using Microsoft.AspNetCore.SignalR;
using Serenity.Services;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Http;

namespace SerenityProjem.Administration;

public class UserActivityService : IUserActivityService
{
    private readonly IHubContext<UserActivityHub> _hubContext;
    private static readonly ConcurrentDictionary<string, OnlineUserInfo> OnlineUsers = new();

    public UserActivityService(IHubContext<UserActivityHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task UserConnectedAsync(string userId, string username, string ipAddress, string userAgent)
    {
        var userInfo = new OnlineUserInfo
        {
            UserId = userId,
            Username = username,
            ConnectionTime = DateTime.UtcNow,
            LastActivity = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Location = await GetLocationFromIpAsync(ipAddress)
        };

        OnlineUsers.AddOrUpdate(userId, userInfo, (key, existing) => userInfo);

        await _hubContext.Clients.All.SendAsync("UserConnected", userInfo);
        await _hubContext.Clients.All.SendAsync("OnlineUsersCountChanged", OnlineUsers.Count);
    }

    public async Task UserDisconnectedAsync(string userId)
    {
        if (OnlineUsers.TryRemove(userId, out var userInfo))
        {
            await _hubContext.Clients.All.SendAsync("UserDisconnected", userInfo);
            await _hubContext.Clients.All.SendAsync("OnlineUsersCountChanged", OnlineUsers.Count);
        }
    }

    public async Task UpdateUserActivityAsync(string userId)
    {
        if (OnlineUsers.TryGetValue(userId, out var userInfo))
        {
            userInfo.LastActivity = DateTime.UtcNow;
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

    private async Task<string> GetLocationFromIpAsync(string ipAddress)
    {
        try
        {
            if (ipAddress == "::1" || ipAddress == "127.0.0.1" || ipAddress.StartsWith("192.168") || ipAddress.StartsWith("10."))
            {
                return "Local Network";
            }

            using var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync($"http://ip-api.com/json/{ipAddress}?fields=country,city");
            var locationData = JsonSerializer.Deserialize<LocationData>(response);
            
            return $"{locationData?.City}, {locationData?.Country}";
        }
        catch
        {
            return "Unknown";
        }
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
}

public class LocationData
{
    public string Country { get; set; } = "";
    public string City { get; set; } = "";
}