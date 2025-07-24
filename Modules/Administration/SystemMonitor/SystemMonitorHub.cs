using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Serenity.Data;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using Dapper;

namespace SerenityProjem.Administration.SystemMonitor;

[Authorize] // Sadece giriş yapmış kullanıcılar
public class SystemMonitorHub : Hub
{
    private readonly ISqlConnections _sqlConnections;
    private readonly ILogger<SystemMonitorHub> _logger;

    public SystemMonitorHub(ISqlConnections sqlConnections, ILogger<SystemMonitorHub> logger)
    {
        _sqlConnections = sqlConnections;
        _logger = logger;
    }

    public async Task JoinMonitoringGroup()
    {
        // Admin kullanıcıları monitoring grubuna ekle
        if (IsUserAdmin())
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "SystemMonitors");
            _logger.LogInformation($"[SystemMonitorHub] User {Context.User.Identity.Name} joined monitoring group");
        }
    }

    public async Task LeaveMonitoringGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SystemMonitors");
        _logger.LogInformation($"[SystemMonitorHub] User {Context.User.Identity.Name} left monitoring group");
    }

    public async Task GetSystemStatus()
    {
        if (!IsUserAdmin()) return;

        var systemStatus = await CollectSystemMetrics();
        await Clients.Caller.SendAsync("SystemStatusUpdate", systemStatus);
    }

    public async Task GetHangfireStatus()
    {
        if (!IsUserAdmin()) return;

        var hangfireStatus = await CollectHangfireMetrics();
        await Clients.Caller.SendAsync("HangfireStatusUpdate", hangfireStatus);
    }

    private bool IsUserAdmin()
    {
        var user = Context.User?.Identity?.Name?.ToLowerInvariant();
        return user == "admin" || Context.User.IsInRole("Administrators");
    }

    private async Task<object> CollectSystemMetrics()
    {
        try
        {
            using var connection = _sqlConnections.NewByKey("Default");
            
            // Sistem metrikleri
            var process = Process.GetCurrentProcess();
            
            // CPU usage'ı basit şekilde hesapla
            var startTime = DateTime.UtcNow;
            var startCpuUsage = process.TotalProcessorTime;
            await Task.Delay(100);
            var endTime = DateTime.UtcNow;
            var endCpuUsage = process.TotalProcessorTime;
            
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed) * 100;
            
            var metrics = new
            {
                Timestamp = DateTime.UtcNow,
                System = new
                {
                    CpuUsage = Math.Round(cpuUsageTotal, 2),
                    MemoryUsageMB = Math.Round(process.WorkingSet64 / 1024.0 / 1024.0, 2),
                    ThreadCount = process.Threads.Count,
                    HandleCount = process.HandleCount
                },
                Database = new
                {
                    ActiveConnections = GetActiveConnectionCount(),
                    DatabaseSize = await GetDatabaseSizeAsync(connection)
                },
                Application = new
                {
                    OnlineUsers = GetOnlineUserCount(),
                    TotalUsers = GetTotalUserCount(),
                    TodayLogins = GetTodayLoginCount(),
                    ActiveSessions = GetActiveSessionCount()
                }
            };

            // Metrics'i veritabanına kaydet
            await SaveSystemHealthMetric(connection, metrics);

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SystemMonitor] Error collecting system metrics");
            return new { Error = "Failed to collect system metrics" };
        }
    }

    private Task<object> CollectHangfireMetrics()
    {
        try
        {
            using var connection = _sqlConnections.NewByKey("Default");
            
            // Hangfire job statistics
            var stats = new
            {
                Timestamp = DateTime.UtcNow,
                Jobs = new
                {
                    Enqueued = GetHangfireJobCount("Enqueued"),
                    Processing = GetHangfireJobCount("Processing"), 
                    Failed = GetHangfireJobCount("Failed"),
                    Succeeded = GetHangfireJobCount("Succeeded"),
                    Recurring = GetRecurringJobCount()
                },
                Queues = GetQueueStatistics(),
                Servers = GetServerStatistics()
            };

            return Task.FromResult<object>(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SystemMonitor] Error collecting Hangfire metrics");
            return Task.FromResult<object>(new { Error = "Failed to collect Hangfire metrics" });
        }
    }

    private int GetActiveConnectionCount()
    {
        try
        {
            using var connection = _sqlConnections.NewByKey("Default");
            var sql = @"SELECT COUNT(*) FROM sys.dm_exec_connections 
                       WHERE session_id > 50"; // System connections excluded
            return Serenity.Data.SqlMapper.Query<int>(connection, sql).FirstOrDefault();
        }
        catch
        {
            return 0;
        }
    }

    private async Task<double> GetDatabaseSizeAsync(IDbConnection connection)
    {
        try
        {
            var sql = @"SELECT SUM(CAST(FILEPROPERTY(name, 'SpaceUsed') AS bigint) * 8192.) / (1024 * 1024) 
                       FROM sys.database_files WHERE type_desc = 'ROWS'";
            var result = await connection.QueryFirstOrDefaultAsync<double>(sql);
            return Math.Round(result, 2);
        }
        catch
        {
            return 0;
        }
    }

    private int GetOnlineUserCount()
    {
        try
        {
            using var connection = _sqlConnections.NewByKey("Default");
            var sql = @"SELECT COUNT(DISTINCT Username) 
                       FROM UserActivityHistory 
                       WHERE ActivityTime >= @Since";
            return Serenity.Data.SqlMapper.Query<int>(connection, sql, new { 
                Since = DateTime.UtcNow.AddMinutes(-15) 
            }).FirstOrDefault();
        }
        catch
        {
            return 0;
        }
    }

    private int GetTotalUserCount()
    {
        try
        {
            using var connection = _sqlConnections.NewByKey("Default");
            return Serenity.Data.SqlMapper.Query<int>(connection, "SELECT COUNT(*) FROM Users").FirstOrDefault();
        }
        catch
        {
            return 0;
        }
    }

    private int GetTodayLoginCount()
    {
        try
        {
            using var connection = _sqlConnections.NewByKey("Default");
            var sql = @"SELECT COUNT(*) FROM UserActivityHistory 
                       WHERE ActivityType = 'Login' 
                       AND CAST(ActivityTime AS DATE) = CAST(GETUTCDATE() AS DATE)";
            return Serenity.Data.SqlMapper.Query<int>(connection, sql).FirstOrDefault();
        }
        catch
        {
            return 0;
        }
    }

    private int GetActiveSessionCount()
    {
        // ASP.NET session count - implementation depends on session provider
        return Context.GetHttpContext()?.Session != null ? 1 : 0; // Simplified
    }

    private int GetHangfireJobCount(string state)
    {
        try
        {
            using var connection = _sqlConnections.NewByKey("Default");
            var sql = $"SELECT COUNT(*) FROM HangFire.Job WHERE StateName = @state";
            return Serenity.Data.SqlMapper.Query<int>(connection, sql, new { state }).FirstOrDefault();
        }
        catch
        {
            return 0;
        }
    }

    private int GetRecurringJobCount()
    {
        try
        {
            using var connection = _sqlConnections.NewByKey("Default");
            var sql = "SELECT COUNT(*) FROM HangFire.Set WHERE [Key] LIKE 'recurring-jobs'";
            return Serenity.Data.SqlMapper.Query<int>(connection, sql).FirstOrDefault();
        }
        catch
        {
            return 0;
        }
    }

    private object GetQueueStatistics()
    {
        var queues = new[] { "default", "maintenance", "reports", "security", "monitoring" };
        var queueStats = new Dictionary<string, int>();

        try
        {
            using var connection = _sqlConnections.NewByKey("Default");
            foreach (var queue in queues)
            {
                var sql = "SELECT COUNT(*) FROM HangFire.JobQueue WHERE Queue = @queue";
                var count = Serenity.Data.SqlMapper.Query<int>(connection, sql, new { queue }).FirstOrDefault();
                queueStats[queue] = count;
            }
        }
        catch
        {
            foreach (var queue in queues)
                queueStats[queue] = 0;
        }

        return queueStats;
    }

    private object GetServerStatistics()
    {
        try
        {
            using var connection = _sqlConnections.NewByKey("Default");
            var sql = @"SELECT Id, Data, HeartBeat 
                       FROM HangFire.Server 
                       WHERE HeartBeat >= @Since";
            
            var servers = Serenity.Data.SqlMapper.Query(connection, sql, new { 
                Since = DateTime.UtcNow.AddMinutes(-1) 
            }).ToList();

            return new
            {
                ActiveServers = servers.Count,
                LastHeartBeat = servers.Any() ? servers.Max(s => s.HeartBeat) : (DateTime?)null
            };
        }
        catch
        {
            return new { ActiveServers = 0, LastHeartBeat = (DateTime?)null };
        }
    }

    private async Task SaveSystemHealthMetric(IDbConnection connection, object metrics)
    {
        try
        {
            var systemMetrics = ((dynamic)metrics).System;
            var sql = @"INSERT INTO SystemHealthMetrics 
                       (Timestamp, CpuUsage, MemoryUsageMB, ActiveConnections, CreatedAt)
                       VALUES (@Timestamp, @CpuUsage, @MemoryUsageMB, @ActiveConnections, @CreatedAt)";

            await connection.ExecuteAsync(sql, new
            {
                Timestamp = DateTime.UtcNow,
                CpuUsage = (double)systemMetrics.CpuUsage,
                MemoryUsageMB = (int)systemMetrics.MemoryUsageMB,
                ActiveConnections = ((dynamic)metrics).Database.ActiveConnections,
                CreatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SystemMonitor] Failed to save system health metric");
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SystemMonitors");
        await base.OnDisconnectedAsync(exception);
    }
}