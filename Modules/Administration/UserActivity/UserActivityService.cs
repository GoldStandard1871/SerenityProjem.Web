using Microsoft.AspNetCore.SignalR;
using Serenity.Services;
using Serenity.Data;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http;

namespace SerenityProjem.Administration;

public class UserActivityService : IUserActivityService
{
    private readonly IHubContext<UserActivityHub> _hubContext;
    private readonly ISqlConnections _sqlConnections;
    private static readonly ConcurrentDictionary<string, OnlineUserInfo> OnlineUsers = new();
    private static readonly ConcurrentDictionary<string, DateTime> LastLoginTimes = new();

    public UserActivityService(IHubContext<UserActivityHub> hubContext, ISqlConnections sqlConnections)
    {
        _hubContext = hubContext;
        _sqlConnections = sqlConnections;
    }

    public async Task UserConnectedAsync(string userId, string username, string ipAddress, string userAgent)
    {
        Console.WriteLine($"[UserActivityService] UserConnectedAsync called for {username} ({userId})");
        
        var locationInfo = await GetDetailedLocationFromIpAsync(ipAddress);
        var userInfo = new OnlineUserInfo
        {
            UserId = userId,
            Username = username,
            ConnectionTime = DateTime.UtcNow,
            LastActivity = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Location = locationInfo.Location,
            Isp = locationInfo.Isp,
            Timezone = locationInfo.Timezone
        };

        // Check if this is a real login (not just SignalR reconnection)
        var isRealLogin = false;
        if (!OnlineUsers.ContainsKey(userId))
        {
            isRealLogin = true;
        }
        else if (LastLoginTimes.TryGetValue(userId, out var lastLogin))
        {
            // If more than 5 minutes since last login, consider it a new session
            isRealLogin = DateTime.UtcNow - lastLogin > TimeSpan.FromMinutes(5);
        }

        OnlineUsers.AddOrUpdate(userId, userInfo, (key, existing) => userInfo);
        Console.WriteLine($"[UserActivityService] User {username} added to online users. Total count: {OnlineUsers.Count}");

        // Only log to database if it's a real login
        if (isRealLogin)
        {
            LastLoginTimes.AddOrUpdate(userId, DateTime.UtcNow, (key, existing) => DateTime.UtcNow);
            
            try
            {
                using var connection = _sqlConnections.NewByKey("Default");
                var sql = @"INSERT INTO UserActivityHistory 
                           (UserId, Username, ActivityType, IpAddress, UserAgent, Location, Isp, Timezone, ActivityTime, Details)
                           VALUES (@UserId, @Username, @ActivityType, @IpAddress, @UserAgent, @Location, @Isp, @Timezone, @ActivityTime, @Details)";
                
                connection.Execute(sql, new {
                    UserId = userId,
                    Username = username,
                    ActivityType = "Login",
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Location = locationInfo.Location,
                    Isp = locationInfo.Isp,
                    Timezone = locationInfo.Timezone,
                    ActivityTime = DateTime.UtcNow,
                    Details = JsonSerializer.Serialize(new { ConnectionTime = userInfo.ConnectionTime, IsRealLogin = true })
                });
                Console.WriteLine($"[UserActivityService] Real login logged for {username}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserActivityService] Error logging activity: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine($"[UserActivityService] SignalR reconnection detected for {username} - not logging as new login");
        }

        await _hubContext.Clients.All.SendAsync("UserConnected", userInfo);
        await _hubContext.Clients.All.SendAsync("OnlineUsersCountChanged", OnlineUsers.Count);
        Console.WriteLine($"[UserActivityService] SignalR notifications sent for {username}");
    }

    public async Task UserDisconnectedAsync(string userId)
    {
        Console.WriteLine($"[UserActivityService] UserDisconnectedAsync called for userId: {userId}");
        
        if (OnlineUsers.TryGetValue(userId, out var userInfo))
        {
            // Check if this is the last connection for this user
            var sessionDuration = DateTime.UtcNow - userInfo.ConnectionTime;
            var isRealLogout = sessionDuration > TimeSpan.FromMinutes(1); // Only log if session was longer than 1 minute
            
            // Remove from online users only if it's a real disconnect
            if (isRealLogout)
            {
                OnlineUsers.TryRemove(userId, out _);
                Console.WriteLine($"[UserActivityService] User {userInfo.Username} ({userId}) removed from online users. Remaining count: {OnlineUsers.Count}");
                
                // Log logout to database using simple SQL
                try
                {
                    using var connection = _sqlConnections.NewByKey("Default");
                    var sql = @"INSERT INTO UserActivityHistory 
                               (UserId, Username, ActivityType, IpAddress, UserAgent, Location, Isp, Timezone, ActivityTime, Details)
                               VALUES (@UserId, @Username, @ActivityType, @IpAddress, @UserAgent, @Location, @Isp, @Timezone, @ActivityTime, @Details)";
                    
                    connection.Execute(sql, new {
                        UserId = userId,
                        Username = userInfo.Username,
                        ActivityType = "Logout",
                        IpAddress = userInfo.IpAddress,
                        UserAgent = userInfo.UserAgent,
                        Location = userInfo.Location,
                        Isp = userInfo.Isp,
                        Timezone = userInfo.Timezone,
                        ActivityTime = DateTime.UtcNow,
                        Details = JsonSerializer.Serialize(new { 
                            ConnectionTime = userInfo.ConnectionTime,
                            DisconnectionTime = DateTime.UtcNow,
                            SessionDuration = sessionDuration.TotalMinutes
                        })
                    });
                    Console.WriteLine($"[UserActivityService] Real logout logged for {userInfo.Username}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[UserActivityService] Error logging logout: {ex.Message}");
                }
                
                await _hubContext.Clients.All.SendAsync("UserDisconnected", userInfo);
                await _hubContext.Clients.All.SendAsync("OnlineUsersCountChanged", OnlineUsers.Count);
                
                Console.WriteLine($"[UserActivityService] SignalR notifications sent for disconnect of {userInfo.Username}");
            }
            else
            {
                Console.WriteLine($"[UserActivityService] SignalR reconnection/refresh detected for {userInfo.Username} - not logging as logout");
            }
        }
        else
        {
            Console.WriteLine($"[UserActivityService] User {userId} was not found in online users list when trying to disconnect");
        }
    }

    public async Task UpdateUserActivityAsync(string userId)
    {
        if (OnlineUsers.TryGetValue(userId, out var userInfo))
        {
            userInfo.LastActivity = DateTime.UtcNow;
            
            // TODO: Database heartbeat logging will be implemented later
            
            await _hubContext.Clients.All.SendAsync("UserActivityUpdated", userInfo);
        }
    }

    public List<OnlineUserInfo> GetOnlineUsers()
    {
        return OnlineUsers.Values.ToList();
    }

    public int GetOnlineUsersCount()
    {
        return OnlineUsers.Count;
    }

    private async Task<(string Location, string Isp, string Timezone)> GetDetailedLocationFromIpAsync(string ipAddress)
    {
        try
        {
            if (ipAddress == "::1" || ipAddress == "127.0.0.1" || ipAddress.StartsWith("192.168") || ipAddress.StartsWith("10."))
            {
                return ("🏠 Local Network", "Local ISP", "Local Time");
            }

            using var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync($"http://ip-api.com/json/{ipAddress}?fields=country,city,regionName,isp,timezone");
            var locationData = JsonSerializer.Deserialize<DetailedLocationData>(response);
            
            if (locationData != null && !string.IsNullOrEmpty(locationData.Country))
            {
                var location = "";
                if (!string.IsNullOrEmpty(locationData.City))
                    location += $"🏙️ {locationData.City}";
                if (!string.IsNullOrEmpty(locationData.RegionName))
                    location += string.IsNullOrEmpty(location) ? $"🗺️ {locationData.RegionName}" : $", {locationData.RegionName}";
                if (!string.IsNullOrEmpty(locationData.Country))
                    location += string.IsNullOrEmpty(location) ? $"🌍 {locationData.Country}" : $", {locationData.Country}";
                
                return (location, locationData.Isp ?? "Unknown ISP", locationData.Timezone ?? "Unknown Timezone");
            }
            
            return ("🌐 Unknown Location", "Unknown ISP", "Unknown Timezone");
        }
        catch
        {
            return ("❓ Unknown", "Unknown ISP", "Unknown Timezone");
        }
    }

    private async Task<string> GetLocationFromIpAsync(string ipAddress)
    {
        var (location, _, _) = await GetDetailedLocationFromIpAsync(ipAddress);
        return location;
    }
}

public interface IUserActivityService
{
    Task UserConnectedAsync(string userId, string username, string ipAddress, string userAgent);
    Task UserDisconnectedAsync(string userId);
    Task UpdateUserActivityAsync(string userId);
    List<OnlineUserInfo> GetOnlineUsers();
    int GetOnlineUsersCount();
}

public class OnlineUserInfo
{
    public string UserId { get; set; } = "";
    public string Username { get; set; } = "";
    public DateTime ConnectionTime { get; set; }
    public DateTime LastActivity { get; set; }
    public string IpAddress { get; set; } = "";
    public string UserAgent { get; set; } = "";
    public string Location { get; set; } = "";
    public string Isp { get; set; } = "";
    public string Timezone { get; set; } = "";
}

public class LocationData
{
    public string Country { get; set; } = "";
    public string City { get; set; } = "";
}

public class DetailedLocationData
{
    public string Country { get; set; } = "";
    public string City { get; set; } = "";
    public string RegionName { get; set; } = "";
    public string Isp { get; set; } = "";
    public string Timezone { get; set; } = "";
}