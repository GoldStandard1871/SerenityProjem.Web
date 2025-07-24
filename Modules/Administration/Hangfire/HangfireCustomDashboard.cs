using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serenity.Abstractions;
using System.Linq;

namespace SerenityProjem.Administration.Hangfire;

public static class HangfireCustomDashboard
{
    public static void ConfigureHangfireDashboard(this IApplicationBuilder app, IServiceProvider serviceProvider)
    {
        var userAccessor = serviceProvider.GetRequiredService<IUserAccessor>();
        
        var dashboardOptions = new DashboardOptions
        {
            Authorization = new[]
            {
                new HangfireDashboardAuthorizationFilter(userAccessor)
            },
            
            // Dashboard özelleştirmeleri
            StatsPollingInterval = 2000, // 2 saniyede bir istatistik güncelle
            DisplayStorageConnectionString = false, // Connection string'i gizle
            
            // Custom CSS ve JS ekleyebiliriz
            AppPath = "/", // Ana sayfa linki
            
            // Dashboard başlığı
            DashboardTitle = "SerenityProjem - System Monitor"
        };

        // Hangfire Dashboard'u /hangfire path'inde aç
        app.UseHangfireDashboard("/hangfire", dashboardOptions);
        
        // Custom metrics endpoint'i
        app.UseHangfireDashboard("/system-monitor", new DashboardOptions
        {
            Authorization = dashboardOptions.Authorization,
            AppPath = "/hangfire",
            DashboardTitle = "System Health Monitor"
        });
    }

    public static void AddCustomHangfireMetrics()
    {
        // Custom metric'ler ekle
        DashboardMetrics.AddMetric(new DashboardMetric(
            "active_users", 
            "Aktif Kullanıcılar",
            page => new Metric(GetActiveUserCount().ToString())
        ));

        DashboardMetrics.AddMetric(new DashboardMetric(
            "movie_count",
            "Toplam Film",
            page => new Metric(GetMovieCount().ToString())
        ));

        DashboardMetrics.AddMetric(new DashboardMetric(
            "person_count",
            "Toplam Kişi", 
            page => new Metric(GetPersonCount().ToString())
        ));

        DashboardMetrics.AddMetric(new DashboardMetric(
            "daily_logins",
            "Bugünkü Giriş",
            page => new Metric(GetTodayLoginCount().ToString())
        ));
    }

    private static int GetActiveUserCount()
    {
        try
        {
            using var services = GetServiceScope();
            var sqlConnections = services.ServiceProvider.GetRequiredService<Serenity.Data.ISqlConnections>();
            using var connection = sqlConnections.NewByKey("Default");
            
            var sql = @"SELECT COUNT(DISTINCT Username) 
                       FROM UserActivityHistory 
                       WHERE ActivityTime >= @Since";
            
            var result = Serenity.Data.SqlMapper.Query<int>(connection, sql, new { 
                Since = DateTime.UtcNow.AddMinutes(-15) 
            });
            return result.FirstOrDefault();
        }
        catch
        {
            return 0;
        }
    }

    private static int GetMovieCount()
    {
        try
        {
            using var services = GetServiceScope();
            var sqlConnections = services.ServiceProvider.GetRequiredService<Serenity.Data.ISqlConnections>();
            using var connection = sqlConnections.NewByKey("Default");
            
            var result = Serenity.Data.SqlMapper.Query<int>(connection, "SELECT COUNT(*) FROM Movie");
            return result.FirstOrDefault();
        }
        catch
        {
            return 0;
        }
    }

    private static int GetPersonCount()
    {
        try
        {
            using var services = GetServiceScope();
            var sqlConnections = services.ServiceProvider.GetRequiredService<Serenity.Data.ISqlConnections>();
            using var connection = sqlConnections.NewByKey("Default");
            
            var result = Serenity.Data.SqlMapper.Query<int>(connection, "SELECT COUNT(*) FROM Person");
            return result.FirstOrDefault();
        }
        catch
        {
            return 0;
        }
    }

    private static int GetTodayLoginCount()
    {
        try
        {
            using var services = GetServiceScope();
            var sqlConnections = services.ServiceProvider.GetRequiredService<Serenity.Data.ISqlConnections>();
            using var connection = sqlConnections.NewByKey("Default");
            
            var sql = @"SELECT COUNT(*) 
                       FROM UserActivityHistory 
                       WHERE ActivityType = 'Login' 
                       AND CAST(ActivityTime AS DATE) = CAST(GETUTCDATE() AS DATE)";
            
            var result = Serenity.Data.SqlMapper.Query<int>(connection, sql);
            return result.FirstOrDefault();
        }
        catch
        {
            return 0;
        }
    }

    private static IServiceScope GetServiceScope()
    {
        // Service provider'a erişim için geçici çözüm
        // Startup'ta static reference tutulabilir
        var httpContextAccessor = ServiceProviderAccessor.Current?.GetService<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        return httpContextAccessor?.HttpContext?.RequestServices?.CreateScope()
               ?? throw new InvalidOperationException("Cannot access service provider");
    }
}

// Service provider erişimi için yardımcı class
public static class ServiceProviderAccessor
{
    public static IServiceProvider Current { get; set; }
}