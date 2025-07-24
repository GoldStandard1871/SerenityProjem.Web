using System.Text;
using System.Text.Json;
using System.Globalization;

namespace SerenityProjem.Administration.ActivityReports;

public static class ReportExporter
{
    public static string ExportToCSV(object reportData, string reportType)
    {
        var csv = new StringBuilder();
        var data = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(reportData));
        
        switch (reportType.ToLower())
        {
            case "daily":
                return ExportDailyReportToCSV(data);
            case "weekly":
                return ExportWeeklyReportToCSV(data);
            case "monthly":
                return ExportMonthlyReportToCSV(data);
            default:
                return ExportGenericToCSV(data);
        }
    }

    private static string ExportDailyReportToCSV(JsonElement data)
    {
        var csv = new StringBuilder();
        
        // Header
        csv.AppendLine("Daily Activity Report");
        csv.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine();
        
        // Summary
        if (data.TryGetProperty("Summary", out var summary))
        {
            csv.AppendLine("SUMMARY");
            csv.AppendLine("Metric,Value");
            csv.AppendLine($"Unique Users,{GetPropertyValue(summary, "UniqueUsers")}");
            csv.AppendLine($"Total Activities,{GetPropertyValue(summary, "TotalActivities")}");
            csv.AppendLine($"Total Logins,{GetPropertyValue(summary, "Logins")}");
            csv.AppendLine($"Page Views,{GetPropertyValue(summary, "PageViews")}");
            csv.AppendLine();
        }
        
        // Hourly Activity
        if (data.TryGetProperty("HourlyActivity", out var hourlyActivity) && hourlyActivity.ValueKind == JsonValueKind.Array)
        {
            csv.AppendLine("HOURLY ACTIVITY");
            csv.AppendLine("Hour,Activity Count,Unique Users");
            
            foreach (var hour in hourlyActivity.EnumerateArray())
            {
                csv.AppendLine($"{GetPropertyValue(hour, "Hour")}:00,{GetPropertyValue(hour, "ActivityCount")},{GetPropertyValue(hour, "UniqueUsers")}");
            }
            csv.AppendLine();
        }
        
        // Top Users
        if (data.TryGetProperty("TopUsers", out var topUsers) && topUsers.ValueKind == JsonValueKind.Array)
        {
            csv.AppendLine("TOP USERS");
            csv.AppendLine("Username,Activity Count,First Activity,Last Activity");
            
            foreach (var user in topUsers.EnumerateArray())
            {
                csv.AppendLine($"{GetPropertyValue(user, "Username")},{GetPropertyValue(user, "ActivityCount")},{GetPropertyValue(user, "FirstActivity")},{GetPropertyValue(user, "LastActivity")}");
            }
            csv.AppendLine();
        }
        
        // Activity Types
        if (data.TryGetProperty("ActivityTypes", out var activityTypes) && activityTypes.ValueKind == JsonValueKind.Array)
        {
            csv.AppendLine("ACTIVITY TYPES");
            csv.AppendLine("Activity Type,Count,Percentage");
            
            foreach (var actType in activityTypes.EnumerateArray())
            {
                csv.AppendLine($"{GetPropertyValue(actType, "ActivityType")},{GetPropertyValue(actType, "Count")},{GetPropertyValue(actType, "Percentage")}%");
            }
            csv.AppendLine();
        }
        
        // Page Views
        if (data.TryGetProperty("PageViews", out var pageViews) && pageViews.ValueKind == JsonValueKind.Array)
        {
            csv.AppendLine("TOP PAGE VIEWS");
            csv.AppendLine("Page,View Count,Unique Viewers");
            
            foreach (var page in pageViews.EnumerateArray())
            {
                csv.AppendLine($"{GetPropertyValue(page, "ActivityDetail")},{GetPropertyValue(page, "ViewCount")},{GetPropertyValue(page, "UniqueViewers")}");
            }
        }
        
        return csv.ToString();
    }

    private static string ExportWeeklyReportToCSV(JsonElement data)
    {
        var csv = new StringBuilder();
        
        // Header
        csv.AppendLine("Weekly Activity Report");
        csv.AppendLine($"Period: {GetPropertyValue(data, "StartDate")} to {GetPropertyValue(data, "EndDate")}");
        csv.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine();
        
        // Summary
        if (data.TryGetProperty("Summary", out var summary))
        {
            csv.AppendLine("WEEKLY SUMMARY");
            csv.AppendLine("Metric,Value");
            csv.AppendLine($"Unique Users,{GetPropertyValue(summary, "UniqueUsers")}");
            csv.AppendLine($"Total Activities,{GetPropertyValue(summary, "TotalActivities")}");
            csv.AppendLine($"Total Logins,{GetPropertyValue(summary, "Logins")}");
            csv.AppendLine($"Avg Daily Activity,{GetPropertyValue(summary, "AvgDailyActivity")}");
            csv.AppendLine();
        }
        
        // Daily Trends
        if (data.TryGetProperty("DailyTrends", out var dailyTrends) && dailyTrends.ValueKind == JsonValueKind.Array)
        {
            csv.AppendLine("DAILY TRENDS");
            csv.AppendLine("Date,Activity Count,Unique Users");
            
            foreach (var day in dailyTrends.EnumerateArray())
            {
                csv.AppendLine($"{GetPropertyValue(day, "Date")},{GetPropertyValue(day, "ActivityCount")},{GetPropertyValue(day, "UniqueUsers")}");
            }
            csv.AppendLine();
        }
        
        // Peak Hours
        if (data.TryGetProperty("PeakHours", out var peakHours) && peakHours.ValueKind == JsonValueKind.Array)
        {
            csv.AppendLine("PEAK HOURS");
            csv.AppendLine("Hour,Activity Count");
            
            foreach (var hour in peakHours.EnumerateArray())
            {
                csv.AppendLine($"{GetPropertyValue(hour, "Hour")}:00,{GetPropertyValue(hour, "ActivityCount")}");
            }
        }
        
        return csv.ToString();
    }

    private static string ExportMonthlyReportToCSV(JsonElement data)
    {
        var csv = new StringBuilder();
        
        // Header
        csv.AppendLine("Monthly Activity Report");
        csv.AppendLine($"Period: {GetPropertyValue(data, "MonthName")}");
        csv.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine();
        
        // Summary
        if (data.TryGetProperty("Summary", out var summary))
        {
            csv.AppendLine("MONTHLY SUMMARY");
            csv.AppendLine("Metric,Value");
            csv.AppendLine($"Unique Users,{GetPropertyValue(summary, "UniqueUsers")}");
            csv.AppendLine($"Total Activities,{GetPropertyValue(summary, "TotalActivities")}");
            csv.AppendLine($"Active Days,{GetPropertyValue(summary, "ActiveDays")}");
            csv.AppendLine($"Avg Daily Activity,{GetPropertyValue(summary, "AvgDailyActivity")}");
            csv.AppendLine();
        }
        
        // Weekly Breakdown
        if (data.TryGetProperty("WeeklyBreakdown", out var weeklyBreakdown) && weeklyBreakdown.ValueKind == JsonValueKind.Array)
        {
            csv.AppendLine("WEEKLY BREAKDOWN");
            csv.AppendLine("Week Number,Week Start,Week End,Activity Count,Unique Users");
            
            foreach (var week in weeklyBreakdown.EnumerateArray())
            {
                csv.AppendLine($"{GetPropertyValue(week, "WeekNumber")},{GetPropertyValue(week, "WeekStart")},{GetPropertyValue(week, "WeekEnd")},{GetPropertyValue(week, "ActivityCount")},{GetPropertyValue(week, "UniqueUsers")}");
            }
            csv.AppendLine();
        }
        
        // User Segmentation
        if (data.TryGetProperty("UserSegmentation", out var userSegmentation) && userSegmentation.ValueKind == JsonValueKind.Array)
        {
            csv.AppendLine("USER SEGMENTATION");
            csv.AppendLine("Segment,User Count,Avg Activity");
            
            foreach (var segment in userSegmentation.EnumerateArray())
            {
                csv.AppendLine($"{GetPropertyValue(segment, "Segment")},{GetPropertyValue(segment, "UserCount")},{GetPropertyValue(segment, "AvgActivity")}");
            }
        }
        
        return csv.ToString();
    }

    private static string ExportGenericToCSV(JsonElement data)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Generic Report Export");
        csv.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        csv.AppendLine();
        csv.AppendLine("Data:");
        csv.AppendLine(data.ToString());
        return csv.ToString();
    }

    private static string GetPropertyValue(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property))
        {
            return property.ValueKind switch
            {
                JsonValueKind.String => property.GetString() ?? "",
                JsonValueKind.Number => property.GetDecimal().ToString(CultureInfo.InvariantCulture),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => "",
                _ => property.ToString()
            };
        }
        return "";
    }

    public static string GenerateHTMLReport(object reportData, string reportType)
    {
        var html = new StringBuilder();
        var data = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(reportData));
        
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head>");
        html.AppendLine("<title>Activity Report</title>");
        html.AppendLine("<style>");
        html.AppendLine(@"
            body { font-family: Arial, sans-serif; margin: 20px; }
            .header { border-bottom: 2px solid #333; padding-bottom: 10px; margin-bottom: 20px; }
            .summary { background: #f5f5f5; padding: 15px; border-radius: 5px; margin-bottom: 20px; }
            .section { margin-bottom: 30px; }
            .section h3 { color: #333; border-bottom: 1px solid #ccc; padding-bottom: 5px; }
            table { width: 100%; border-collapse: collapse; margin-bottom: 20px; }
            th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
            th { background-color: #f2f2f2; }
            tr:nth-child(even) { background-color: #f9f9f9; }
            .metric { display: inline-block; margin: 10px 20px 10px 0; }
            .metric-value { font-size: 1.5em; font-weight: bold; color: #2c5aa0; }
            .metric-label { font-size: 0.9em; color: #666; }
        ");
        html.AppendLine("</style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        
        // Header
        html.AppendLine("<div class='header'>");
        html.AppendLine($"<h1>Activity Report - {reportType.ToUpper()}</h1>");
        html.AppendLine($"<p>Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
        html.AppendLine("</div>");
        
        // Add specific content based on report type
        switch (reportType.ToLower())
        {
            case "daily":
                AppendDailyHTMLContent(html, data);
                break;
            case "weekly":
                AppendWeeklyHTMLContent(html, data);
                break;
            case "monthly":
                AppendMonthlyHTMLContent(html, data);
                break;
        }
        
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        
        return html.ToString();
    }

    private static void AppendDailyHTMLContent(StringBuilder html, JsonElement data)
    {
        // Summary
        if (data.TryGetProperty("Summary", out var summary))
        {
            html.AppendLine("<div class='summary'>");
            html.AppendLine("<h2>Daily Summary</h2>");
            html.AppendLine($"<div class='metric'><div class='metric-value'>{GetPropertyValue(summary, "UniqueUsers")}</div><div class='metric-label'>Unique Users</div></div>");
            html.AppendLine($"<div class='metric'><div class='metric-value'>{GetPropertyValue(summary, "TotalActivities")}</div><div class='metric-label'>Total Activities</div></div>");
            html.AppendLine($"<div class='metric'><div class='metric-value'>{GetPropertyValue(summary, "Logins")}</div><div class='metric-label'>Total Logins</div></div>");
            html.AppendLine($"<div class='metric'><div class='metric-value'>{GetPropertyValue(summary, "PageViews")}</div><div class='metric-label'>Page Views</div></div>");
            html.AppendLine("</div>");
        }
        
        // Top Users Table
        if (data.TryGetProperty("TopUsers", out var topUsers) && topUsers.ValueKind == JsonValueKind.Array)
        {
            html.AppendLine("<div class='section'>");
            html.AppendLine("<h3>Top Active Users</h3>");
            html.AppendLine("<table>");
            html.AppendLine("<tr><th>Username</th><th>Activity Count</th><th>First Activity</th><th>Last Activity</th></tr>");
            
            foreach (var user in topUsers.EnumerateArray())
            {
                html.AppendLine($"<tr><td>{GetPropertyValue(user, "Username")}</td><td>{GetPropertyValue(user, "ActivityCount")}</td><td>{GetPropertyValue(user, "FirstActivity")}</td><td>{GetPropertyValue(user, "LastActivity")}</td></tr>");
            }
            
            html.AppendLine("</table>");
            html.AppendLine("</div>");
        }
    }

    private static void AppendWeeklyHTMLContent(StringBuilder html, JsonElement data)
    {
        // Weekly Summary
        if (data.TryGetProperty("Summary", out var summary))
        {
            html.AppendLine("<div class='summary'>");
            html.AppendLine("<h2>Weekly Summary</h2>");
            html.AppendLine($"<p><strong>Period:</strong> {GetPropertyValue(data, "StartDate")} to {GetPropertyValue(data, "EndDate")}</p>");
            html.AppendLine($"<div class='metric'><div class='metric-value'>{GetPropertyValue(summary, "UniqueUsers")}</div><div class='metric-label'>Unique Users</div></div>");
            html.AppendLine($"<div class='metric'><div class='metric-value'>{GetPropertyValue(summary, "TotalActivities")}</div><div class='metric-label'>Total Activities</div></div>");
            html.AppendLine($"<div class='metric'><div class='metric-value'>{GetPropertyValue(summary, "Logins")}</div><div class='metric-label'>Total Logins</div></div>");
            html.AppendLine("</div>");
        }
        
        // Daily Trends Table
        if (data.TryGetProperty("DailyTrends", out var dailyTrends) && dailyTrends.ValueKind == JsonValueKind.Array)
        {
            html.AppendLine("<div class='section'>");
            html.AppendLine("<h3>Daily Activity Trends</h3>");
            html.AppendLine("<table>");
            html.AppendLine("<tr><th>Date</th><th>Activity Count</th><th>Unique Users</th></tr>");
            
            foreach (var day in dailyTrends.EnumerateArray())
            {
                html.AppendLine($"<tr><td>{GetPropertyValue(day, "Date")}</td><td>{GetPropertyValue(day, "ActivityCount")}</td><td>{GetPropertyValue(day, "UniqueUsers")}</td></tr>");
            }
            
            html.AppendLine("</table>");
            html.AppendLine("</div>");
        }
    }

    private static void AppendMonthlyHTMLContent(StringBuilder html, JsonElement data)
    {
        // Monthly Summary
        if (data.TryGetProperty("Summary", out var summary))
        {
            html.AppendLine("<div class='summary'>");
            html.AppendLine("<h2>Monthly Summary</h2>");
            html.AppendLine($"<p><strong>Period:</strong> {GetPropertyValue(data, "MonthName")}</p>");
            html.AppendLine($"<div class='metric'><div class='metric-value'>{GetPropertyValue(summary, "UniqueUsers")}</div><div class='metric-label'>Unique Users</div></div>");
            html.AppendLine($"<div class='metric'><div class='metric-value'>{GetPropertyValue(summary, "TotalActivities")}</div><div class='metric-label'>Total Activities</div></div>");
            html.AppendLine($"<div class='metric'><div class='metric-value'>{GetPropertyValue(summary, "ActiveDays")}</div><div class='metric-label'>Active Days</div></div>");
            html.AppendLine("</div>");
        }
        
        // User Segmentation Table
        if (data.TryGetProperty("UserSegmentation", out var userSegmentation) && userSegmentation.ValueKind == JsonValueKind.Array)
        {
            html.AppendLine("<div class='section'>");
            html.AppendLine("<h3>User Activity Segmentation</h3>");
            html.AppendLine("<table>");
            html.AppendLine("<tr><th>Segment</th><th>User Count</th><th>Average Activity</th></tr>");
            
            foreach (var segment in userSegmentation.EnumerateArray())
            {
                html.AppendLine($"<tr><td>{GetPropertyValue(segment, "Segment")}</td><td>{GetPropertyValue(segment, "UserCount")}</td><td>{GetPropertyValue(segment, "AvgActivity")}</td></tr>");
            }
            
            html.AppendLine("</table>");
            html.AppendLine("</div>");
        }
    }
}