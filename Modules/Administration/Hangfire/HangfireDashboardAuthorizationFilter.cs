using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using Serenity.Abstractions;
using Serenity.Services;

namespace SerenityProjem.Administration.Hangfire;

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly IUserAccessor _userAccessor;

    public HangfireDashboardAuthorizationFilter(IUserAccessor userAccessor)
    {
        _userAccessor = userAccessor;
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        
        // Development ortamında herkes erişebilir
        if (IsInDevelopment())
            return true;

        try
        {
            var user = _userAccessor.User;
            
            // Kullanıcı giriş yapmamışsa erişim yok
            if (user == null || !user.Identity.IsAuthenticated)
                return false;

            // Admin yetkisi kontrolü
            var username = user.Identity.Name;
            if (IsUserAdmin(username))
                return true;

            // System Monitor yetkisi kontrolü (özel permission)
            if (HasSystemMonitorPermission(user))
                return true;

            return false;
        }
        catch
        {
            return false;
        }
    }

    private bool IsInDevelopment()
    {
        // Environment check - production'da false döner
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return env == "Development";
    }

    private bool IsUserAdmin(string username)
    {
        // Admin kullanıcı kontrolü - bu daha detaylı implement edilebilir
        return username?.ToLowerInvariant() == "admin";
    }

    private bool HasSystemMonitorPermission(System.Security.Claims.ClaimsPrincipal user)
    {
        // Custom permission kontrolü - System Monitor yetkisi
        return user.HasClaim("permission", "Administration:SystemMonitor") ||
               user.HasClaim("permission", "Administration:General");
    }
}