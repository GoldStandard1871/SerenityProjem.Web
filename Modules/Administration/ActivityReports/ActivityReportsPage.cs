using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Serenity.Web;
using Serenity.Data;
using System.Data;
using System.Globalization;
using System.Text;
using SerenityProjem.Administration.ActivityReports;

namespace SerenityProjem.Administration.Pages;

[PageAuthorize("Administration:General")]
public class ActivityReportsController : Controller
{
    private readonly ISqlConnections _sqlConnections;

    public ActivityReportsController(ISqlConnections sqlConnections)
    {
        _sqlConnections = sqlConnections;
    }

    [Route("Administration/ActivityReports")]
    public ActionResult Index()
    {
        return View(MVC.Views.Administration.ActivityReports.ActivityReportsPage);
    }

    [HttpGet]
    [Route("api/activity-reports/daily")]
    public JsonResult GetDailyReport(DateTime? date = null)
    {
        var targetDate = date ?? DateTime.Today;
        
        try
        {
            using var connection = _sqlConnections.NewByKey("Default");
            
            var report = new
            {
                Date = targetDate.ToString("yyyy-MM-dd"),
                Summary = GetDailySummary(connection, targetDate),
                HourlyActivity = GetHourlyActivity(connection, targetDate),
                TopUsers = GetTopUsersByActivity(connection, targetDate),
                ActivityTypes = GetActivityTypeDistribution(connection, targetDate),
                PageViews = GetTopPageViews(connection, targetDate)
            };

            return new JsonResult(report);
        }
        catch (Exception ex)
        {
            return new JsonResult(new { Error = "Failed to generate daily report", Message = ex.Message });
        }
    }

    [HttpGet]
    [Route("api/activity-reports/weekly")]
    public JsonResult GetWeeklyReport(DateTime? startDate = null)
    {
        var weekStart = startDate ?? DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
        var weekEnd = weekStart.AddDays(6);
        
        try
        {
            using var connection = _sqlConnections.NewByKey("Default");
            
            var report = new
            {
                StartDate = weekStart.ToString("yyyy-MM-dd"),
                EndDate = weekEnd.ToString("yyyy-MM-dd"),
                Summary = GetWeeklySummary(connection, weekStart, weekEnd),
                DailyTrends = GetDailyTrends(connection, weekStart, weekEnd),
                UserGrowth = GetUserGrowthTrend(connection, weekStart, weekEnd),
                PeakHours = GetPeakHours(connection, weekStart, weekEnd)
            };

            return new JsonResult(report);
        }
        catch (Exception ex)
        {
            return new JsonResult(new { Error = "Failed to generate weekly report", Message = ex.Message });
        }
    }

    [HttpGet]
    [Route("api/activity-reports/monthly")]
    public JsonResult GetMonthlyReport(int? year = null, int? month = null)
    {
        var targetDate = new DateTime(year ?? DateTime.Now.Year, month ?? DateTime.Now.Month, 1);
        var monthStart = targetDate;
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        
        try
        {
            using var connection = _sqlConnections.NewByKey("Default");
            
            var report = new
            {
                Year = targetDate.Year,
                Month = targetDate.Month,
                MonthName = targetDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                Summary = GetMonthlySummary(connection, monthStart, monthEnd),
                WeeklyBreakdown = GetWeeklyBreakdown(connection, monthStart, monthEnd),
                UserSegmentation = GetUserSegmentation(connection, monthStart, monthEnd),
                ActivityHeatmap = GetActivityHeatmap(connection, monthStart, monthEnd)
            };

            return new JsonResult(report);
        }
        catch (Exception ex)
        {
            return new JsonResult(new { Error = "Failed to generate monthly report", Message = ex.Message });
        }
    }

    [HttpGet]
    [Route("api/activity-reports/overview")]
    public JsonResult GetOverviewReport()
    {
        try
        {
            using var connection = _sqlConnections.NewByKey("Default");
            
            var report = new
            {
                Summary = GetOverallSummary(connection),
                RecentActivities = GetRecentActivities(connection, 50),
                ActivityDistribution = GetOverallActivityDistribution(connection),
                UserActivitySummary = GetUserActivitySummary(connection),
                ActiveLocations = GetActiveLocations(connection),
                SystemHealth = GetSystemHealthMetrics(connection),
                PageViews = GetTopPageViewsOverall(connection),
                ActivityTypes = GetOverallActivityDistribution(connection),
                HourlyDistribution = GetOverallHourlyDistribution(connection)
            };

            return new JsonResult(report);
        }
        catch (Exception ex)
        {
            return new JsonResult(new { Error = "Failed to generate overview report", Message = ex.Message });
        }
    }

    private object GetDailySummary(IDbConnection connection, DateTime date)
    {
        var sql = @"
            SELECT 
                COUNT(DISTINCT Username) as UniqueUsers,
                COUNT(*) as TotalActivities,
                COUNT(CASE WHEN ActivityType = 'Login' THEN 1 END) as Logins,
                COUNT(CASE WHEN ActivityType = 'Logout' THEN 1 END) as Logouts,
                COUNT(CASE WHEN ActivityType = 'PageView' THEN 1 END) as PageViews
            FROM UserActivityHistory 
            WHERE CAST(ActivityTime AS DATE) = @Date";

        return Serenity.Data.SqlMapper.Query(connection, sql, new { Date = date }).FirstOrDefault();
    }

    private object[] GetHourlyActivity(IDbConnection connection, DateTime date)
    {
        var sql = @"
            SELECT 
                DATEPART(HOUR, ActivityTime) as Hour,
                COUNT(*) as ActivityCount,
                COUNT(DISTINCT Username) as UniqueUsers
            FROM UserActivityHistory 
            WHERE CAST(ActivityTime AS DATE) = @Date
            GROUP BY DATEPART(HOUR, ActivityTime)
            ORDER BY Hour";

        return Serenity.Data.SqlMapper.Query(connection, sql, new { Date = date }).ToArray();
    }

    private object[] GetTopUsersByActivity(IDbConnection connection, DateTime date)
    {
        var sql = @"
            SELECT TOP 10
                Username,
                COUNT(*) as ActivityCount,
                MIN(ActivityTime) as FirstActivity,
                MAX(ActivityTime) as LastActivity
            FROM UserActivityHistory 
            WHERE CAST(ActivityTime AS DATE) = @Date
            GROUP BY Username
            ORDER BY ActivityCount DESC";

        return Serenity.Data.SqlMapper.Query(connection, sql, new { Date = date }).ToArray();
    }

    private object[] GetActivityTypeDistribution(IDbConnection connection, DateTime date)
    {
        var sql = @"
            SELECT 
                ActivityType,
                COUNT(*) as Count,
                CAST(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER() AS DECIMAL(5,2)) as Percentage
            FROM UserActivityHistory 
            WHERE CAST(ActivityTime AS DATE) = @Date
            GROUP BY ActivityType
            ORDER BY Count DESC";

        return Serenity.Data.SqlMapper.Query(connection, sql, new { Date = date }).ToArray();
    }

    private object[] GetTopPageViews(IDbConnection connection, DateTime date)
    {
        var sql = @"
            SELECT TOP 10
                Details as ActivityDetail,
                COUNT(*) as ViewCount,
                COUNT(DISTINCT Username) as UniqueViewers
            FROM UserActivityHistory 
            WHERE CAST(ActivityTime AS DATE) = @Date 
            AND ActivityType = 'PageView'
            AND Details IS NOT NULL
            GROUP BY Details
            ORDER BY ViewCount DESC";

        return Serenity.Data.SqlMapper.Query(connection, sql, new { Date = date }).ToArray();
    }

    private object GetWeeklySummary(IDbConnection connection, DateTime startDate, DateTime endDate)
    {
        var sql = @"
            SELECT 
                COUNT(DISTINCT Username) as UniqueUsers,
                COUNT(*) as TotalActivities,
                COUNT(CASE WHEN ActivityType = 'Login' THEN 1 END) as Logins,
                AVG(CAST(COUNT(*) as FLOAT)) OVER() as AvgDailyActivity
            FROM UserActivityHistory 
            WHERE ActivityTime >= @StartDate AND ActivityTime <= @EndDate";

        return Serenity.Data.SqlMapper.Query(connection, sql, new { StartDate = startDate, EndDate = endDate }).FirstOrDefault();
    }

    private object[] GetDailyTrends(IDbConnection connection, DateTime startDate, DateTime endDate)
    {
        var sql = @"
            SELECT 
                CAST(ActivityTime AS DATE) as Date,
                COUNT(*) as ActivityCount,
                COUNT(DISTINCT Username) as UniqueUsers
            FROM UserActivityHistory 
            WHERE ActivityTime >= @StartDate AND ActivityTime <= @EndDate
            GROUP BY CAST(ActivityTime AS DATE)
            ORDER BY Date";

        return Serenity.Data.SqlMapper.Query(connection, sql, new { StartDate = startDate, EndDate = endDate }).ToArray();
    }

    private object[] GetUserGrowthTrend(IDbConnection connection, DateTime startDate, DateTime endDate)
    {
        var sql = @"
            SELECT 
                CAST(ActivityTime AS DATE) as Date,
                COUNT(DISTINCT Username) as ActiveUsers,
                COUNT(DISTINCT CASE WHEN ActivityType = 'Login' THEN Username END) as LoginUsers
            FROM UserActivityHistory 
            WHERE ActivityTime >= @StartDate AND ActivityTime <= @EndDate
            GROUP BY CAST(ActivityTime AS DATE)
            ORDER BY Date";

        return Serenity.Data.SqlMapper.Query(connection, sql, new { StartDate = startDate, EndDate = endDate }).ToArray();
    }

    private object[] GetPeakHours(IDbConnection connection, DateTime startDate, DateTime endDate)
    {
        var sql = @"
            SELECT 
                DATEPART(HOUR, ActivityTime) as Hour,
                COUNT(*) as ActivityCount,
                AVG(CAST(COUNT(*) as FLOAT)) OVER() as AvgHourlyActivity
            FROM UserActivityHistory 
            WHERE ActivityTime >= @StartDate AND ActivityTime <= @EndDate
            GROUP BY DATEPART(HOUR, ActivityTime)
            ORDER BY ActivityCount DESC";

        return Serenity.Data.SqlMapper.Query(connection, sql, new { StartDate = startDate, EndDate = endDate }).ToArray();
    }

    private object GetMonthlySummary(IDbConnection connection, DateTime startDate, DateTime endDate)
    {
        var sql = @"
            SELECT 
                COUNT(DISTINCT Username) as UniqueUsers,
                COUNT(*) as TotalActivities,
                COUNT(DISTINCT CAST(ActivityTime AS DATE)) as ActiveDays,
                AVG(CAST(COUNT(*) as FLOAT)) OVER() as AvgDailyActivity
            FROM UserActivityHistory 
            WHERE ActivityTime >= @StartDate AND ActivityTime <= @EndDate";

        return Serenity.Data.SqlMapper.Query(connection, sql, new { StartDate = startDate, EndDate = endDate }).FirstOrDefault();
    }

    private object[] GetWeeklyBreakdown(IDbConnection connection, DateTime startDate, DateTime endDate)
    {
        var sql = @"
            SELECT 
                DATEPART(WEEK, ActivityTime) as WeekNumber,
                MIN(CAST(ActivityTime AS DATE)) as WeekStart,
                MAX(CAST(ActivityTime AS DATE)) as WeekEnd,
                COUNT(*) as ActivityCount,
                COUNT(DISTINCT Username) as UniqueUsers
            FROM UserActivityHistory 
            WHERE ActivityTime >= @StartDate AND ActivityTime <= @EndDate
            GROUP BY DATEPART(WEEK, ActivityTime)
            ORDER BY WeekNumber";

        return Serenity.Data.SqlMapper.Query(connection, sql, new { StartDate = startDate, EndDate = endDate }).ToArray();
    }

    private object[] GetUserSegmentation(IDbConnection connection, DateTime startDate, DateTime endDate)
    {
        var sql = @"
            WITH UserActivityCounts AS (
                SELECT 
                    Username,
                    COUNT(*) as ActivityCount
                FROM UserActivityHistory 
                WHERE ActivityTime >= @StartDate AND ActivityTime <= @EndDate
                GROUP BY Username
            )
            SELECT 
                CASE 
                    WHEN ActivityCount >= 100 THEN 'High Activity (100+)'
                    WHEN ActivityCount >= 50 THEN 'Medium Activity (50-99)'
                    WHEN ActivityCount >= 10 THEN 'Low Activity (10-49)'
                    ELSE 'Minimal Activity (<10)'
                END as Segment,
                COUNT(*) as UserCount,
                AVG(CAST(ActivityCount as FLOAT)) as AvgActivity
            FROM UserActivityCounts
            GROUP BY CASE 
                WHEN ActivityCount >= 100 THEN 'High Activity (100+)'
                WHEN ActivityCount >= 50 THEN 'Medium Activity (50-99)'
                WHEN ActivityCount >= 10 THEN 'Low Activity (10-49)'
                ELSE 'Minimal Activity (<10)'
            END
            ORDER BY AvgActivity DESC";

        return Serenity.Data.SqlMapper.Query(connection, sql, new { StartDate = startDate, EndDate = endDate }).ToArray();
    }

    private object[] GetActivityHeatmap(IDbConnection connection, DateTime startDate, DateTime endDate)
    {
        var sql = @"
            SELECT 
                DATEPART(WEEKDAY, ActivityTime) as DayOfWeek,
                DATEPART(HOUR, ActivityTime) as Hour,
                COUNT(*) as ActivityCount
            FROM UserActivityHistory 
            WHERE ActivityTime >= @StartDate AND ActivityTime <= @EndDate
            GROUP BY DATEPART(WEEKDAY, ActivityTime), DATEPART(HOUR, ActivityTime)
            ORDER BY DayOfWeek, Hour";

        return Serenity.Data.SqlMapper.Query(connection, sql, new { StartDate = startDate, EndDate = endDate }).ToArray();
    }

    [HttpGet]
    [Route("api/activity-reports/export/csv")]
    public IActionResult ExportToCSV(string reportType, string reportDate, string format = "csv")
    {
        try
        {
            // Validate parameters
            if (string.IsNullOrEmpty(reportType))
            {
                return BadRequest("Report type is required");
            }
            
            if (string.IsNullOrEmpty(reportDate))
            {
                return BadRequest("Report date is required");
            }
            
            // Get report data first
            object reportData = null;
            switch (reportType.ToLower())
            {
                case "daily":
                    var dailyResult = GetDailyReport(DateTime.Parse(reportDate));
                    reportData = ((JsonResult)dailyResult).Value;
                    break;
                case "weekly":
                    var weeklyResult = GetWeeklyReport(DateTime.Parse(reportDate));
                    reportData = ((JsonResult)weeklyResult).Value;
                    break;
                case "monthly":
                    var parts = reportDate.Split('-');
                    var monthlyResult = GetMonthlyReport(int.Parse(parts[0]), int.Parse(parts[1]));
                    reportData = ((JsonResult)monthlyResult).Value;
                    break;
                default:
                    return BadRequest("Invalid report type");
            }

            if (reportData == null)
            {
                return NotFound("Report data not found");
            }

            string content;
            string contentType;
            string fileName;

            switch (format.ToLower())
            {
                case "csv":
                    content = ReportExporter.ExportToCSV(reportData, reportType);
                    contentType = "text/csv";
                    fileName = $"activity-report-{reportType}-{reportDate}.csv";
                    break;
                case "html":
                    content = ReportExporter.GenerateHTMLReport(reportData, reportType);
                    contentType = "text/html";
                    fileName = $"activity-report-{reportType}-{reportDate}.html";
                    break;
                default:
                    return BadRequest("Invalid format. Supported formats: csv, html");
            }

            var bytes = Encoding.UTF8.GetBytes(content);
            return File(bytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Export failed", Message = ex.Message });
        }
    }

    [HttpGet] 
    [Route("api/activity-reports/export/html")]
    public IActionResult ExportToHTML(string reportType, string reportDate)
    {
        return ExportToCSV(reportType, reportDate, "html");
    }

    [HttpPost]
    [Route("api/activity-reports/schedule")]
    public JsonResult ScheduleReport(string reportType, string frequency, string email)
    {
        try
        {
            // This would integrate with Hangfire for scheduling
            // For now, return a success response
            var scheduledReport = new
            {
                Id = Guid.NewGuid().ToString(),
                ReportType = reportType,
                Frequency = frequency,
                Email = email,
                CreatedAt = DateTime.UtcNow,
                NextRun = CalculateNextRun(frequency),
                Status = "Active"
            };

            // Here you would save to database and schedule with Hangfire
            // RecurringJob.AddOrUpdate($"scheduled-report-{scheduledReport.Id}", 
            //     () => GenerateAndEmailReport(reportType, email), 
            //     GetCronExpression(frequency));

            return new JsonResult(new { Success = true, ScheduledReport = scheduledReport });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { Success = false, Error = ex.Message });
        }
    }

    private DateTime CalculateNextRun(string frequency)
    {
        return frequency.ToLower() switch
        {
            "daily" => DateTime.Today.AddDays(1).AddHours(9), // 9 AM tomorrow
            "weekly" => DateTime.Today.AddDays(7 - (int)DateTime.Today.DayOfWeek + 1).AddHours(9), // Next Monday 9 AM
            "monthly" => new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddHours(9), // 1st of next month 9 AM
            _ => DateTime.Today.AddDays(1).AddHours(9)
        };
    }

    private object GetOverallSummary(IDbConnection connection)
    {
        var sql = @"
            SELECT 
                COUNT(DISTINCT Username) as UniqueUsers,
                COUNT(*) as TotalActivities,
                COUNT(DISTINCT CAST(ActivityTime AS DATE)) as ActiveDays,
                COUNT(CASE WHEN ActivityType = 'Login' THEN 1 END) as TotalLogins,
                COUNT(CASE WHEN ActivityType = 'Logout' THEN 1 END) as TotalLogouts,
                COUNT(CASE WHEN ActivityType = 'PageView' THEN 1 END) as TotalPageViews,
                COUNT(CASE WHEN ActivityTime >= DATEADD(DAY, -1, GETDATE()) THEN 1 END) as Last24HourActivities,
                COUNT(CASE WHEN ActivityTime >= DATEADD(DAY, -7, GETDATE()) THEN 1 END) as Last7DayActivities
            FROM UserActivityHistory";

        return Serenity.Data.SqlMapper.Query(connection, sql).FirstOrDefault();
    }

    private object[] GetRecentActivities(IDbConnection connection, int limit)
    {
        var sql = @"
            SELECT TOP (@Limit)
                Username,
                ActivityType,
                ActivityTime,
                Details,
                Location,
                IpAddress
            FROM UserActivityHistory 
            ORDER BY ActivityTime DESC";

        return Serenity.Data.SqlMapper.Query(connection, sql, new { Limit = limit }).ToArray();
    }

    private object[] GetOverallActivityDistribution(IDbConnection connection)
    {
        var sql = @"
            SELECT 
                ActivityType,
                COUNT(*) as Count,
                CAST(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER() AS DECIMAL(5,2)) as Percentage
            FROM UserActivityHistory 
            GROUP BY ActivityType
            ORDER BY Count DESC";

        return Serenity.Data.SqlMapper.Query(connection, sql).ToArray();
    }

    private object[] GetUserActivitySummary(IDbConnection connection)
    {
        var sql = @"
            SELECT 
                Username,
                COUNT(*) as TotalActivities,
                COUNT(DISTINCT CAST(ActivityTime AS DATE)) as ActiveDays,
                MIN(ActivityTime) as FirstActivity,
                MAX(ActivityTime) as LastActivity,
                COUNT(CASE WHEN ActivityType = 'Login' THEN 1 END) as LoginCount,
                COUNT(CASE WHEN ActivityType = 'PageView' THEN 1 END) as PageViewCount
            FROM UserActivityHistory 
            GROUP BY Username
            ORDER BY TotalActivities DESC";

        return Serenity.Data.SqlMapper.Query(connection, sql).ToArray();
    }

    private object[] GetActiveLocations(IDbConnection connection)
    {
        var sql = @"
            SELECT TOP 10
                Location,
                COUNT(*) as ActivityCount,
                COUNT(DISTINCT Username) as UniqueUsers
            FROM UserActivityHistory 
            WHERE Location IS NOT NULL AND Location != ''
            GROUP BY Location
            ORDER BY ActivityCount DESC";

        return Serenity.Data.SqlMapper.Query(connection, sql).ToArray();
    }

    private object GetSystemHealthMetrics(IDbConnection connection)
    {
        var now = DateTime.Now;
        var sql = @"
            SELECT 
                (SELECT COUNT(*) FROM UserActivityHistory WHERE ActivityTime >= DATEADD(MINUTE, -5, GETDATE())) as ActivitiesLast5Min,
                (SELECT COUNT(DISTINCT Username) FROM UserActivityHistory WHERE ActivityTime >= DATEADD(HOUR, -1, GETDATE())) as ActiveUsersLastHour,
                (SELECT TOP 1 ActivityTime FROM UserActivityHistory ORDER BY ActivityTime DESC) as LastActivityTime,
                (SELECT COUNT(*) FROM UserActivityHistory WHERE ActivityType = 'Login' AND ActivityTime >= DATEADD(DAY, -1, GETDATE())) as LoginsLast24Hours";

        var metrics = Serenity.Data.SqlMapper.Query(connection, sql).FirstOrDefault();
        
        // Calculate system status
        var lastActivity = metrics?.LastActivityTime ?? DateTime.MinValue;
        var timeSinceLastActivity = now - lastActivity;
        var status = timeSinceLastActivity.TotalMinutes switch
        {
            < 5 => "Active",
            < 30 => "Normal",
            < 60 => "Slow",
            _ => "Inactive"
        };

        return new
        {
            Status = status,
            ActivitiesLast5Min = metrics?.ActivitiesLast5Min ?? 0,
            ActiveUsersLastHour = metrics?.ActiveUsersLastHour ?? 0,
            LoginsLast24Hours = metrics?.LoginsLast24Hours ?? 0,
            LastActivityTime = lastActivity,
            TimeSinceLastActivity = $"{(int)timeSinceLastActivity.TotalMinutes} minutes ago"
        };
    }

    private object[] GetTopPageViewsOverall(IDbConnection connection)
    {
        var sql = @"
            SELECT TOP 10
                Details as ActivityDetail,
                COUNT(*) as ViewCount,
                COUNT(DISTINCT Username) as UniqueViewers
            FROM UserActivityHistory 
            WHERE ActivityType = 'PageView'
            AND Details IS NOT NULL
            GROUP BY Details
            ORDER BY ViewCount DESC";

        return Serenity.Data.SqlMapper.Query(connection, sql).ToArray();
    }

    private object[] GetOverallHourlyDistribution(IDbConnection connection)
    {
        var sql = @"
            SELECT 
                DATEPART(HOUR, ActivityTime) as Hour,
                COUNT(*) as ActivityCount,
                COUNT(DISTINCT Username) as UniqueUsers
            FROM UserActivityHistory 
            WHERE ActivityTime >= DATEADD(DAY, -7, GETDATE())
            GROUP BY DATEPART(HOUR, ActivityTime)
            ORDER BY Hour";

        return Serenity.Data.SqlMapper.Query(connection, sql).ToArray();
    }
}