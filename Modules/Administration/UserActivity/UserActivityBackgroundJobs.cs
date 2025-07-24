using Hangfire;
using Microsoft.Extensions.Logging;
using Serenity.Data;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace SerenityProjem.Administration;

public class UserActivityBackgroundJobs
{
    private readonly ISqlConnections _sqlConnections;
    private readonly ILogger<UserActivityBackgroundJobs> _logger;

    public UserActivityBackgroundJobs(ISqlConnections sqlConnections, ILogger<UserActivityBackgroundJobs> logger)
    {
        _sqlConnections = sqlConnections;
        _logger = logger;
    }

    /// <summary>
    /// Eski aktivite kayıtlarını temizler (30 günden eski)
    /// </summary>
    [Queue("maintenance")]
    public void CleanupOldActivityRecords()
    {
        try
        {
            using var connection = _sqlConnections.NewByKey("Default");
            
            var cutoffDate = DateTime.UtcNow.AddDays(-30);
            var sql = @"DELETE FROM UserActivityHistory 
                       WHERE ActivityTime < @CutoffDate";
            
            var deletedCount = Serenity.Data.SqlMapper.Execute(connection, sql, new { CutoffDate = cutoffDate });
            
            _logger.LogInformation($"[UserActivityCleanup] {deletedCount} old activity records cleaned up");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UserActivityCleanup] Error during cleanup");
            throw;
        }
    }

    /// <summary>
    /// Günlük aktivite istatistiklerini hesaplar ve saklar
    /// </summary>
    [Queue("reports")]
    public void GenerateDailyActivityStats()
    {
        try
        {
            using var connection = _sqlConnections.NewByKey("Default");
            
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            var today = DateTime.UtcNow.Date;
            
            _logger.LogInformation($"[DailyStats] Starting daily stats generation for {yesterday:yyyy-MM-dd}");
            
            // Günlük istatistikleri hesapla - basitleştirilmiş sorgu
            var sql = @"
                SELECT 
                    COUNT(DISTINCT Username) as UniqueUsers,
                    COUNT(CASE WHEN ActivityType = 'Login' THEN 1 END) as TotalLogins,
                    COUNT(CASE WHEN ActivityType = 'Logout' THEN 1 END) as TotalLogouts,
                    COUNT(DISTINCT Location) as UniqueLocations
                FROM UserActivityHistory
                WHERE ActivityTime >= @StartDate AND ActivityTime < @EndDate";
            
            _logger.LogInformation($"[DailyStats] Executing query for period {yesterday:yyyy-MM-dd} to {today:yyyy-MM-dd}");
            
            var statsResult = Serenity.Data.SqlMapper.Query(connection, sql, new { 
                StartDate = yesterday, 
                EndDate = today 
            });
            var stats = statsResult.FirstOrDefault();
            
            _logger.LogInformation($"[DailyStats] Query executed successfully, found {stats?.UniqueUsers ?? 0} unique users");
            
            // İstatistikleri kaydet (DailyActivityStats tablosu gerekebilir)
            SaveDailyStats(yesterday, stats);
            
            _logger.LogInformation($"[DailyStats] Generated stats for {yesterday:yyyy-MM-dd}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DailyStats] Error generating daily stats");
            throw;
        }
    }

    /// <summary>
    /// Şüpheli aktiviteleri tespit eder ve admin'lere bildirim gönderir
    /// </summary>
    [Queue("security")]
    public void DetectSuspiciousActivity()
    {
        try
        {
            using var connection = _sqlConnections.NewByKey("Default");
            
            // Son 1 saat içinde şüpheli aktiviteler
            var lastHour = DateTime.UtcNow.AddHours(-1);
            
            // 1. Aynı kullanıcı farklı lokasyonlardan giriş
            var suspiciousLogins = Serenity.Data.SqlMapper.Query(connection, @"
                SELECT Username, COUNT(DISTINCT Location) as LocationCount,
                       STRING_AGG(Location, ', ') as Locations
                FROM UserActivityHistory 
                WHERE ActivityType = 'Login' AND ActivityTime >= @LastHour
                GROUP BY Username 
                HAVING COUNT(DISTINCT Location) > 1", new { LastHour = lastHour });

            // 2. Çok fazla başarısız giriş denemesi (eğer implement edilirse)
            var multipleFailedLogins = Serenity.Data.SqlMapper.Query(connection, @"
                SELECT Username, COUNT(*) as LoginCount
                FROM UserActivityHistory 
                WHERE ActivityType = 'Login' AND ActivityTime >= @LastHour
                GROUP BY Username 
                HAVING COUNT(*) > 10", new { LastHour = lastHour });

            if (suspiciousLogins.Any() || multipleFailedLogins.Any())
            {
                NotifyAdminsAboutSuspiciousActivity(suspiciousLogins, multipleFailedLogins);
            }

            _logger.LogInformation($"[SecurityCheck] Suspicious activity check completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SecurityCheck] Error during security check");
            throw;
        }
    }

    /// <summary>
    /// Haftalık aktivite raporunu hazırlar
    /// </summary>
    [Queue("reports")]
    public void GenerateWeeklyReport()
    {
        try
        {
            using var connection = _sqlConnections.NewByKey("Default");
            
            var weekAgo = DateTime.UtcNow.Date.AddDays(-7);
            var today = DateTime.UtcNow.Date;
            
            var reportResult = Serenity.Data.SqlMapper.Query(connection, @"
                SELECT 
                    COUNT(DISTINCT Username) as WeeklyActiveUsers,
                    COUNT(CASE WHEN ActivityType = 'Login' THEN 1 END) as TotalLogins,
                    AVG(CASE WHEN ActivityType = 'Logout' THEN 
                        CAST(JSON_VALUE(Details, '$.SessionDuration') as FLOAT) END) as AvgSessionHours,
                    COUNT(DISTINCT Location) as UniqueLocations,
                    COUNT(DISTINCT IpAddress) as UniqueIPs
                FROM UserActivityHistory 
                WHERE ActivityTime >= @WeekAgo AND ActivityTime < @Today", 
                new { WeekAgo = weekAgo, Today = today });
            var report = reportResult.FirstOrDefault();

            // En aktif kullanıcılar
            var topUsers = Serenity.Data.SqlMapper.Query(connection, @"
                SELECT TOP 10 Username, COUNT(*) as ActivityCount
                FROM UserActivityHistory 
                WHERE ActivityTime >= @WeekAgo AND ActivityTime < @Today
                GROUP BY Username 
                ORDER BY COUNT(*) DESC", 
                new { WeekAgo = weekAgo, Today = today });

            // Konum dağılımı
            var locationStats = Serenity.Data.SqlMapper.Query(connection, @"
                SELECT Location, COUNT(*) as AccessCount
                FROM UserActivityHistory 
                WHERE ActivityTime >= @WeekAgo AND ActivityTime < @Today 
                      AND ActivityType = 'Login'
                GROUP BY Location 
                ORDER BY COUNT(*) DESC", 
                new { WeekAgo = weekAgo, Today = today });

            var reportData = new
            {
                WeekOf = weekAgo.ToString("yyyy-MM-dd"),
                Summary = report,
                TopUsers = topUsers,
                LocationStats = locationStats
            };

            // Raporu email olarak gönder veya dosyaya kaydet
            SaveWeeklyReport(reportData);
            
            _logger.LogInformation($"[WeeklyReport] Generated weekly report for {weekAgo:yyyy-MM-dd}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[WeeklyReport] Error generating weekly report");
            throw;
        }
    }

    /// <summary>
    /// Online kullanıcı sayısını periyodik olarak loglar
    /// </summary>
    [Queue("monitoring")]
    public void LogOnlineUserMetrics()
    {
        try
        {
            // UserActivityService'den online kullanıcı sayısını al
            // Bu istatistikleri zaman serisi olarak kaydet
            using var connection = _sqlConnections.NewByKey("Default");
            
            var onlineCount = GetCurrentOnlineUserCount();
            
            // Önce tablo var mı kontrol et
            var checkTableSql = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                                 WHERE TABLE_NAME = 'OnlineUserMetrics'";
            
            var tableExists = Serenity.Data.SqlMapper.Query<int>(connection, checkTableSql).FirstOrDefault();
            
            if (tableExists == 0)
            {
                _logger.LogWarning("[OnlineMetrics] OnlineUserMetrics table does not exist, skipping save");
                return;
            }
            
            var sql = @"INSERT INTO OnlineUserMetrics (Timestamp, OnlineCount, CreatedAt) 
                       VALUES (@Timestamp, @OnlineCount, @CreatedAt)";
            
            Serenity.Data.SqlMapper.Execute(connection, sql, new { 
                Timestamp = DateTime.UtcNow,
                OnlineCount = onlineCount,
                CreatedAt = DateTime.UtcNow
            });
            
            _logger.LogInformation($"[OnlineMetrics] Logged {onlineCount} online users");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[OnlineMetrics] Error logging online metrics");
            // Bu job critical değil, devam et
        }
    }

    private int GetCurrentOnlineUserCount()
    {
        using var connection = _sqlConnections.NewByKey("Default");
        
        // Son 15 dakika içinde aktivite gösteren kullanıcıları say
        var sql = @"SELECT COUNT(DISTINCT Username) 
                   FROM UserActivityHistory 
                   WHERE ActivityTime >= @Since";
        
        var result = Serenity.Data.SqlMapper.Query<int>(connection, sql, new { 
            Since = DateTime.UtcNow.AddMinutes(-15) 
        });
        return result.FirstOrDefault();
    }

    private void SaveDailyStats(DateTime date, dynamic stats)
    {
        // DailyActivityStats tablosu oluşturulmalı
        using var connection = _sqlConnections.NewByKey("Default");
        
        try
        {
            // Önce tablo var mı kontrol et
            var checkTableSql = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                                 WHERE TABLE_NAME = 'DailyActivityStats'";
            
            var tableExists = Serenity.Data.SqlMapper.Query<int>(connection, checkTableSql).FirstOrDefault();
            
            if (tableExists == 0)
            {
                _logger.LogWarning("[DailyStats] DailyActivityStats table does not exist, skipping save");
                return;
            }
            
            var sql = @"INSERT INTO DailyActivityStats 
                       (Date, UniqueUsers, TotalLogins, TotalLogouts, UniqueLocations, AvgSessionMinutes, CreatedAt)
                       VALUES (@Date, @UniqueUsers, @TotalLogins, @TotalLogouts, @UniqueLocations, @AvgSessionMinutes, @CreatedAt)";
            
            Serenity.Data.SqlMapper.Execute(connection, sql, new {
                Date = date,
                UniqueUsers = stats?.UniqueUsers ?? 0,
                TotalLogins = stats?.TotalLogins ?? 0,
                TotalLogouts = stats?.TotalLogouts ?? 0,
                UniqueLocations = stats?.UniqueLocations ?? 0,
                AvgSessionMinutes = 0, // Şimdilik 0, sonra hesaplanabilir
                CreatedAt = DateTime.UtcNow
            });
            
            _logger.LogInformation($"[DailyStats] Saved daily stats for {date:yyyy-MM-dd}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DailyStats] Error saving daily stats");
        }
    }

    private void NotifyAdminsAboutSuspiciousActivity(dynamic suspiciousLogins, dynamic multipleFailedLogins)
    {
        // Admin kullanıcılarına email gönder veya sistem bildirimi oluştur
        var message = $"Şüpheli aktivite tespit edildi: {DateTime.UtcNow}";
        _logger.LogWarning($"[Security] {message}");
        
        // Burada email gönderme veya bildirim sistemi implement edilebilir
    }

    private void SaveWeeklyReport(object reportData)
    {
        // Raporu JSON olarak kaydet veya email gönder
        var reportJson = JsonSerializer.Serialize(reportData, new JsonSerializerOptions { WriteIndented = true });
        
        // App_Data/Reports klasörüne kaydet
        var reportsPath = Path.Combine("App_Data", "Reports");
        Directory.CreateDirectory(reportsPath);
        
        var fileName = $"WeeklyReport_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
        var filePath = Path.Combine(reportsPath, fileName);
        
        File.WriteAllText(filePath, reportJson);
        
        _logger.LogInformation($"[WeeklyReport] Saved report to {filePath}");
    }
}