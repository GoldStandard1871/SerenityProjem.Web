using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Serenity.Web;
using Serenity.Data;
using System.Data;
using System.Diagnostics;

namespace SerenityProjem.Administration.Pages;

[PageAuthorize("Administration:General")]
public class BackgroundJobsController : Controller
{
    private readonly ISqlConnections _sqlConnections;

    public BackgroundJobsController(ISqlConnections sqlConnections)
    {
        _sqlConnections = sqlConnections;
    }

    [Route("Administration/BackgroundJobs")]
    public ActionResult Index()
    {
        return View(MVC.Views.Administration.BackgroundJobs.BackgroundJobsPage);
    }

    [HttpGet]
    [Route("api/systemhealth")]
    [AllowAnonymous]
    public JsonResult GetSystemHealth()
    {
        try
        {
            using var connection = _sqlConnections.NewByKey("Default");
            var process = Process.GetCurrentProcess();
            
            var healthData = new
            {
                Timestamp = DateTime.UtcNow,
                System = new
                {
                    CpuUsage = Math.Round(GetCpuUsage(), 2),
                    MemoryUsageMB = Math.Round(process.WorkingSet64 / 1024.0 / 1024.0, 2),
                    ThreadCount = process.Threads.Count,
                    HandleCount = process.HandleCount
                },
                Database = new
                {
                    ActiveConnections = GetActiveConnectionCount(),
                    DatabaseSize = GetDatabaseSize(connection)
                },
                Application = new
                {
                    OnlineUsers = GetOnlineUserCount(),
                    TotalUsers = GetTotalUserCount(),
                    TodayLogins = GetTodayLoginCount()
                }
            };

            return new JsonResult(healthData);
        }
        catch (Exception ex)
        {
            return new JsonResult(new { Error = "Failed to collect system health data", Message = ex.Message });
        }
    }

    private double GetCpuUsage()
    {
        // Simple CPU usage calculation
        try
        {
            var process = Process.GetCurrentProcess();
            return (process.TotalProcessorTime.TotalMilliseconds / Environment.ProcessorCount) / 1000.0;
        }
        catch
        {
            return 0;
        }
    }

    private int GetActiveConnectionCount()
    {
        try
        {
            using var connection = _sqlConnections.NewByKey("Default");
            var sql = @"SELECT COUNT(*) FROM sys.dm_exec_connections WHERE session_id > 50";
            return Serenity.Data.SqlMapper.Query<int>(connection, sql).FirstOrDefault();
        }
        catch
        {
            return 0;
        }
    }

    private double GetDatabaseSize(IDbConnection connection)
    {
        try
        {
            var sql = @"SELECT SUM(CAST(FILEPROPERTY(name, 'SpaceUsed') AS bigint) * 8192.) / (1024 * 1024) 
                       FROM sys.database_files WHERE type_desc = 'ROWS'";
            return Math.Round(Serenity.Data.SqlMapper.Query<double>(connection, sql).FirstOrDefault(), 2);
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
}