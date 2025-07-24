using Hangfire;
using Microsoft.Extensions.Logging;
using Serenity.Data;
using System.Data;
using System.Text.Json;
using System.Linq;
using System.IO;

namespace SerenityProjem.Administration.MovieSystem;

public class MovieSystemBackgroundJobs
{
    private readonly ISqlConnections _sqlConnections;
    private readonly ILogger<MovieSystemBackgroundJobs> _logger;

    public MovieSystemBackgroundJobs(ISqlConnections sqlConnections, ILogger<MovieSystemBackgroundJobs> logger)
    {
        _sqlConnections = sqlConnections;
        _logger = logger;
    }

    /// <summary>
    /// Film popülerlik skorlarını günceller
    /// </summary>
    [Queue("reports")]
    public void UpdateMoviePopularityScores()
    {
        try
        {
            using var connection = _sqlConnections.NewByKey("Default");
            
            _logger.LogInformation("[MovieStats] Starting movie popularity score update");

            // Popülerlik skoru hesaplama: cast sayısı + yıl faktörü + genre çeşitliliği
            var sql = @"
                UPDATE MovieStatistics SET 
                    PopularityScore = (
                        -- Cast member count (0-50 points)
                        CASE WHEN CastMemberCount > 10 THEN 50 
                             WHEN CastMemberCount > 5 THEN CastMemberCount * 4
                             ELSE CastMemberCount * 2 END +
                        
                        -- Year factor (newer movies get more points, 0-30 points)
                        CASE WHEN m.Year >= YEAR(GETDATE()) - 5 THEN 30
                             WHEN m.Year >= YEAR(GETDATE()) - 10 THEN 20
                             WHEN m.Year >= YEAR(GETDATE()) - 20 THEN 10
                             ELSE 5 END +
                        
                        -- Genre diversity (0-20 points)
                        COALESCE((SELECT COUNT(*) * 5 FROM MovieGenres mg WHERE mg.MovieId = m.MovieId), 0)
                    ),
                    UpdatedAt = GETUTCDATE()
                FROM MovieStatistics ms
                INNER JOIN Movie m ON ms.MovieId = m.MovieId";

            var updatedCount = Serenity.Data.SqlMapper.Execute(connection, sql);
            
            _logger.LogInformation($"[MovieStats] Updated popularity scores for {updatedCount} movies");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MovieStats] Error updating movie popularity scores");
            throw;
        }
    }

    /// <summary>
    /// Film istatistiklerini günceller (cast sayısı, son erişim vb.)
    /// </summary>
    [Queue("maintenance")]
    public void UpdateMovieStatistics()
    {
        try
        {
            using var connection = _sqlConnections.NewByKey("Default");
            
            _logger.LogInformation("[MovieStats] Starting movie statistics update");

            // Önce eksik MovieStatistics kayıtlarını oluştur
            var createMissingStatsSql = @"
                INSERT INTO MovieStatistics (MovieId, CastMemberCount, UpdatedAt)
                SELECT m.MovieId, 0, GETUTCDATE()
                FROM Movie m
                LEFT JOIN MovieStatistics ms ON m.MovieId = ms.MovieId
                WHERE ms.MovieId IS NULL";

            var createdCount = Serenity.Data.SqlMapper.Execute(connection, createMissingStatsSql);
            if (createdCount > 0)
            {
                _logger.LogInformation($"[MovieStats] Created statistics for {createdCount} new movies");
            }

            // Cast member count'ları güncelle
            var updateCastCountSql = @"
                UPDATE MovieStatistics SET 
                    CastMemberCount = COALESCE((
                        SELECT COUNT(*) 
                        FROM MovieCast mc 
                        WHERE mc.MovieId = MovieStatistics.MovieId
                    ), 0),
                    UpdatedAt = GETUTCDATE()";

            var updatedCount = Serenity.Data.SqlMapper.Execute(connection, updateCastCountSql);
            
            _logger.LogInformation($"[MovieStats] Updated cast member counts for {updatedCount} movies");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MovieStats] Error updating movie statistics");
            throw;
        }
    }

    /// <summary>
    /// En popüler filmleri belirler ve raporlar
    /// </summary>
    [Queue("reports")]
    public void GeneratePopularMoviesReport()
    {
        try
        {
            using var connection = _sqlConnections.NewByKey("Default");
            
            _logger.LogInformation("[MovieStats] Generating popular movies report");

            // Top 10 most popular movies
            var topMoviesSql = @"
                SELECT TOP 10 
                    m.Title,
                    m.Year,
                    ms.PopularityScore,
                    ms.CastMemberCount,
                    STRING_AGG(g.Name, ', ') as Genres
                FROM Movie m
                INNER JOIN MovieStatistics ms ON m.MovieId = ms.MovieId
                LEFT JOIN MovieGenres mg ON m.MovieId = mg.MovieId
                LEFT JOIN Genre g ON mg.GenreId = g.GenreId
                GROUP BY m.MovieId, m.Title, m.Year, ms.PopularityScore, ms.CastMemberCount
                ORDER BY ms.PopularityScore DESC";

            var topMovies = Serenity.Data.SqlMapper.Query(connection, topMoviesSql).ToList();

            // Most active actors (most movies)
            var topActorsSql = @"
                SELECT TOP 10 
                    p.FirstName + ' ' + p.LastName as FullName,
                    COUNT(DISTINCT mc.MovieId) as MovieCount,
                    STRING_AGG(DISTINCT m.Title, ', ') as RecentMovies
                FROM Person p
                INNER JOIN MovieCast mc ON p.PersonId = mc.PersonId
                INNER JOIN Movie m ON mc.MovieId = m.MovieId
                GROUP BY p.PersonId, p.FirstName, p.LastName
                ORDER BY COUNT(DISTINCT mc.MovieId) DESC";

            var topActors = Serenity.Data.SqlMapper.Query(connection, topActorsSql).ToList();

            // Genre distribution
            var genreStatsSql = @"
                SELECT 
                    g.Name as GenreName,
                    COUNT(DISTINCT mg.MovieId) as MovieCount,
                    AVG(CAST(ms.PopularityScore as FLOAT)) as AvgPopularityScore
                FROM Genre g
                LEFT JOIN MovieGenres mg ON g.GenreId = mg.GenreId
                LEFT JOIN MovieStatistics ms ON mg.MovieId = ms.MovieId
                GROUP BY g.GenreId, g.Name
                ORDER BY COUNT(DISTINCT mg.MovieId) DESC";

            var genreStats = Serenity.Data.SqlMapper.Query(connection, genreStatsSql).ToList();

            var report = new
            {
                GeneratedAt = DateTime.UtcNow,
                TopMovies = topMovies,
                TopActors = topActors,
                GenreStatistics = genreStats,
                Summary = new
                {
                    TotalMovies = GetTotalMovieCount(connection),
                    TotalActors = GetTotalActorCount(connection),
                    TotalGenres = GetTotalGenreCount(connection),
                    AvgCastSize = GetAverageCastSize(connection)
                }
            };

            // Raporu kaydet
            SavePopularMoviesReport(report);
            
            _logger.LogInformation($"[MovieStats] Generated popular movies report with {topMovies.Count} top movies");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MovieStats] Error generating popular movies report");
            throw;
        }
    }

    /// <summary>
    /// Film veritabanı sağlık kontrolü yapar
    /// </summary>
    [Queue("maintenance")]
    public void PerformMovieDataHealthCheck()
    {
        try
        {
            using var connection = _sqlConnections.NewByKey("Default");
            
            _logger.LogInformation("[MovieHealth] Starting movie data health check");

            var healthIssues = new List<string>();

            // 1. Movies without cast members
            var moviesWithoutCastSql = @"
                SELECT m.MovieId, m.Title 
                FROM Movie m
                LEFT JOIN MovieCast mc ON m.MovieId = mc.MovieId
                WHERE mc.MovieId IS NULL";

            var moviesWithoutCast = Serenity.Data.SqlMapper.Query(connection, moviesWithoutCastSql).ToList();
            if (moviesWithoutCast.Any())
            {
                healthIssues.Add($"{moviesWithoutCast.Count} movies have no cast members");
            }

            // 2. Movies without genres
            var moviesWithoutGenresSql = @"
                SELECT m.MovieId, m.Title 
                FROM Movie m
                LEFT JOIN MovieGenres mg ON m.MovieId = mg.MovieId
                WHERE mg.MovieId IS NULL";

            var moviesWithoutGenres = Serenity.Data.SqlMapper.Query(connection, moviesWithoutGenresSql).ToList();
            if (moviesWithoutGenres.Any())
            {
                healthIssues.Add($"{moviesWithoutGenres.Count} movies have no genres assigned");
            }

            // 3. Actors with only one movie
            var singleMovieActorsSql = @"
                SELECT p.PersonId, p.FirstName + ' ' + p.LastName as FullName
                FROM Person p
                INNER JOIN MovieCast mc ON p.PersonId = mc.PersonId
                GROUP BY p.PersonId, p.FirstName, p.LastName
                HAVING COUNT(DISTINCT mc.MovieId) = 1";

            var singleMovieActors = Serenity.Data.SqlMapper.Query(connection, singleMovieActorsSql).ToList();
            if (singleMovieActors.Count > 50) // Only report if there are many
            {
                healthIssues.Add($"{singleMovieActors.Count} actors appear in only one movie");
            }

            // 4. Duplicate movie titles
            var duplicateMoviesSql = @"
                SELECT Title, COUNT(*) as Count
                FROM Movie
                GROUP BY Title
                HAVING COUNT(*) > 1";

            var duplicateMovies = Serenity.Data.SqlMapper.Query(connection, duplicateMoviesSql).ToList();
            if (duplicateMovies.Any())
            {
                healthIssues.Add($"{duplicateMovies.Count} duplicate movie titles found");
            }

            // Health report sonucu
            var healthReport = new
            {
                CheckedAt = DateTime.UtcNow,
                HealthStatus = healthIssues.Any() ? "Warning" : "Healthy",
                Issues = healthIssues,
                Statistics = new
                {
                    MoviesWithoutCast = moviesWithoutCast.Count,
                    MoviesWithoutGenres = moviesWithoutGenres.Count,
                    SingleMovieActors = singleMovieActors.Count,
                    DuplicateMovies = duplicateMovies.Count
                }
            };

            SaveHealthCheckReport(healthReport);

            var status = healthIssues.Any() ? "with issues" : "healthy";
            _logger.LogInformation($"[MovieHealth] Health check completed - database is {status}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MovieHealth] Error during movie data health check");
            throw;
        }
    }

    /// <summary>
    /// Eski ve kullanılmayan movie data'ları temizler
    /// </summary>
    [Queue("maintenance")]
    public void CleanupOrphanedMovieData()
    {
        try
        {
            using var connection = _sqlConnections.NewByKey("Default");
            
            _logger.LogInformation("[MovieCleanup] Starting orphaned movie data cleanup");

            // 1. MovieCast records without valid Movie or Person
            var orphanedCastSql = @"
                DELETE mc FROM MovieCast mc
                LEFT JOIN Movie m ON mc.MovieId = m.MovieId
                LEFT JOIN Person p ON mc.PersonId = p.PersonId
                WHERE m.MovieId IS NULL OR p.PersonId IS NULL";

            var deletedCast = Serenity.Data.SqlMapper.Execute(connection, orphanedCastSql);

            // 2. MovieGenres records without valid Movie or Genre
            var orphanedGenresSql = @"
                DELETE mg FROM MovieGenres mg
                LEFT JOIN Movie m ON mg.MovieId = m.MovieId
                LEFT JOIN Genre g ON mg.GenreId = g.GenreId
                WHERE m.MovieId IS NULL OR g.GenreId IS NULL";

            var deletedGenres = Serenity.Data.SqlMapper.Execute(connection, orphanedGenresSql);

            // 3. MovieStatistics without valid Movie
            var orphanedStatsSql = @"
                DELETE ms FROM MovieStatistics ms
                LEFT JOIN Movie m ON ms.MovieId = m.MovieId
                WHERE m.MovieId IS NULL";

            var deletedStats = Serenity.Data.SqlMapper.Execute(connection, orphanedStatsSql);

            _logger.LogInformation($"[MovieCleanup] Cleaned up {deletedCast} cast records, {deletedGenres} genre records, {deletedStats} statistics records");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MovieCleanup] Error during movie data cleanup");
            throw;
        }
    }

    // Helper methods
    private int GetTotalMovieCount(IDbConnection connection)
    {
        return Serenity.Data.SqlMapper.Query<int>(connection, "SELECT COUNT(*) FROM Movie").FirstOrDefault();
    }

    private int GetTotalActorCount(IDbConnection connection)
    {
        return Serenity.Data.SqlMapper.Query<int>(connection, "SELECT COUNT(*) FROM Person").FirstOrDefault();
    }

    private int GetTotalGenreCount(IDbConnection connection)
    {
        return Serenity.Data.SqlMapper.Query<int>(connection, "SELECT COUNT(*) FROM Genre").FirstOrDefault();
    }

    private double GetAverageCastSize(IDbConnection connection)
    {
        var sql = "SELECT AVG(CAST(CastMemberCount as FLOAT)) FROM MovieStatistics WHERE CastMemberCount > 0";
        return Math.Round(Serenity.Data.SqlMapper.Query<double>(connection, sql).FirstOrDefault(), 2);
    }

    private void SavePopularMoviesReport(object report)
    {
        try
        {
            var reportJson = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
            var reportsPath = Path.Combine("App_Data", "Reports", "Movies");
            Directory.CreateDirectory(reportsPath);
            
            var fileName = $"PopularMoviesReport_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
            var filePath = Path.Combine(reportsPath, fileName);
            
            File.WriteAllText(filePath, reportJson);
            _logger.LogInformation($"[MovieStats] Saved popular movies report to {filePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MovieStats] Failed to save popular movies report");
        }
    }

    private void SaveHealthCheckReport(object report)
    {
        try
        {
            var reportJson = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
            var reportsPath = Path.Combine("App_Data", "Reports", "Health");
            Directory.CreateDirectory(reportsPath);
            
            var fileName = $"MovieHealthCheck_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
            var filePath = Path.Combine(reportsPath, fileName);
            
            File.WriteAllText(filePath, reportJson);
            _logger.LogInformation($"[MovieHealth] Saved health check report to {filePath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MovieHealth] Failed to save health check report");
        }
    }
}