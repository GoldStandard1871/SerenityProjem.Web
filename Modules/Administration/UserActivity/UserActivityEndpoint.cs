using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Serenity;
using Serenity.Data;
using Serenity.Reporting;
using Serenity.Services;
using Serenity.Web;
using Dapper;
using MyRequest = Serenity.Services.ListRequest;
using MyResponse = Serenity.Services.ListResponse<SerenityProjem.Administration.OnlineUserInfo>;
using MyRow = SerenityProjem.Administration.UserRow;
using System.Net;

namespace SerenityProjem.Administration.Endpoints;

[Route("Services/Administration/UserActivity/[action]")]
[ConnectionKey("Default")]
[Authorize] // Sadece giriş yapmış kullanıcılar erişebilir
public class UserActivityEndpoint : ServiceEndpoint
{
    private readonly IUserActivityService _userActivityService;

    public UserActivityEndpoint(IUserActivityService userActivityService)
    {
        _userActivityService = userActivityService;
    }

    [HttpPost]
    public MyResponse GetOnlineUsers(MyRequest request)
    {
        var currentUser = Context.User?.Identity?.Name ?? "";
        var isAdmin = Context.User?.IsInRole("Administrators") == true || 
                      Context.User?.IsInRole("admin") == true ||
                      currentUser.ToLower() == "admin";
        
        var onlineUsers = _userActivityService.GetOnlineUsers();
        
        // Normal kullanıcılar sadece kendi bilgilerini görebilir
        if (!isAdmin)
        {
            onlineUsers = onlineUsers.Where(u => u.Username == currentUser).ToList();
        }

        return new MyResponse
        {
            Entities = onlineUsers,
            TotalCount = onlineUsers.Count
        };
    }

    [HttpPost]
    public Result<OnlineUsersCountResponse> GetOnlineUsersCount()
    {
        return this.ExecuteMethod(() =>
        {
            var count = _userActivityService.GetOnlineUsersCount();
            Console.WriteLine($"[UserActivityEndpoint] GetOnlineUsersCount called, returning: {count}");
            return new OnlineUsersCountResponse { Count = count };
        });
    }

    [HttpPost]
    public Result<UpdateActivityResponse> UpdateActivity(UpdateActivityRequest request)
    {
        return this.ExecuteMethod(() =>
        {
            var userId = Context.User?.Identity?.Name ?? "";
            if (!string.IsNullOrEmpty(userId))
            {
                Console.WriteLine($"[UserActivityEndpoint] UpdateActivity called for user: {userId}");
                _userActivityService.UpdateUserActivityAsync(userId);
                return new UpdateActivityResponse { Success = true };
            }
            return new UpdateActivityResponse { Success = false };
        });
    }

    [HttpPost]
    public Result<GetRecentActivityResponse> GetRecentActivity(GetRecentActivityRequest request,
        [FromServices] ISqlConnections sqlConnections)
    {
        return this.ExecuteMethod(() =>
        {
            var currentUser = Context.User?.Identity?.Name ?? "";
            var isAdmin = Context.User?.IsInRole("Administrators") == true || 
                          Context.User?.IsInRole("admin") == true ||
                          currentUser.ToLower() == "admin";
            
            using var connection = sqlConnections.NewByKey("Default");
            
            string sql;
            object parameters;
            
            if (isAdmin)
            {
                // Admin tüm aktiviteleri görebilir
                sql = @"SELECT TOP (@Take) UserId, Username, ActivityType, Location, ActivityTime 
                       FROM UserActivityHistory 
                       WHERE ActivityType IN ('Login', 'Logout')
                       ORDER BY ActivityTime DESC";
                parameters = new { Take = request.Take ?? 20 };
            }
            else
            {
                // Normal kullanıcı sadece kendi aktivitelerini görebilir
                sql = @"SELECT TOP (@Take) UserId, Username, ActivityType, Location, ActivityTime 
                       FROM UserActivityHistory 
                       WHERE ActivityType IN ('Login', 'Logout') AND Username = @Username
                       ORDER BY ActivityTime DESC";
                parameters = new { Take = request.Take ?? 20, Username = currentUser };
            }
            
            var activities = Dapper.SqlMapper.Query<ActivityHistoryItem>(connection, sql, parameters).ToList();
            return new GetRecentActivityResponse { Activities = activities };
        });
    }

    [HttpPost]
    public Result<LogActivityResponse> LogActivity(LogActivityRequest request,
        [FromServices] ISqlConnections sqlConnections,
        [FromServices] IHttpContextAccessor httpContextAccessor)
    {
        return this.ExecuteMethod(() =>
        {
            var username = Context.User?.Identity?.Name ?? "";
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0";
            
            if (string.IsNullOrEmpty(username))
                return new LogActivityResponse { Success = false, Message = "User not authenticated" };

            using var connection = sqlConnections.NewByKey("Default");
            
            // Get client IP and user agent
            var httpContext = httpContextAccessor.HttpContext;
            var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = httpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown";
            
            // Get location from IP (simplified - in production use a geolocation service)
            var location = "Unknown";
            if (ipAddress.StartsWith("127.") || ipAddress.StartsWith("::1"))
                location = "Local";
            
            var sql = @"
                INSERT INTO UserActivityHistory 
                (UserId, Username, ActivityType, Details, ActivityTime, IpAddress, UserAgent, Location)
                VALUES 
                (@UserId, @Username, @ActivityType, @Details, @ActivityTime, @IpAddress, @UserAgent, @Location)";
            
            var parameters = new
            {
                UserId = int.TryParse(userId, out var uid) ? uid : 0,
                Username = username,
                ActivityType = request.ActivityType ?? "Unknown",
                Details = request.Details,
                ActivityTime = DateTime.Now,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Location = location
            };
            
            try
            {
                Dapper.SqlMapper.Execute(connection, sql, parameters);
                return new LogActivityResponse { Success = true };
            }
            catch (Exception ex)
            {
                return new LogActivityResponse 
                { 
                    Success = false, 
                    Message = "Failed to log activity: " + ex.Message 
                };
            }
        });
    }
}

public class OnlineUsersCountResponse : ServiceResponse
{
    public int Count { get; set; }
}

public class UpdateActivityRequest : ServiceRequest
{
    public string LastActivity { get; set; } = "";
}

public class UpdateActivityResponse : ServiceResponse
{
    public bool Success { get; set; }
}

public class GetRecentActivityRequest : ServiceRequest
{
    public int? Take { get; set; }
}

public class GetRecentActivityResponse : ServiceResponse
{
    public List<ActivityHistoryItem> Activities { get; set; } = new List<ActivityHistoryItem>();
}

public class ActivityHistoryItem
{
    public string UserId { get; set; } = "";
    public string Username { get; set; } = "";
    public string ActivityType { get; set; } = "";
    public string Location { get; set; } = "";
    public DateTime ActivityTime { get; set; }
}

public class LogActivityRequest : ServiceRequest
{
    public string ActivityType { get; set; } = "";
    public string Details { get; set; } = "";
    public DateTime? Timestamp { get; set; }
}

public class LogActivityResponse : ServiceResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
}