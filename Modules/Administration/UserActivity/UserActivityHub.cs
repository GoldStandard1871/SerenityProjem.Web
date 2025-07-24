using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace SerenityProjem.Administration;

[Authorize]
public class UserActivityHub : Hub
{
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var username = Context.User?.Identity?.Name;
        
        if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(username))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "OnlineUsers");
            
            // Get UserActivityService from DI
            var serviceProvider = Context.GetHttpContext()?.RequestServices;
            var userActivityService = serviceProvider?.GetService<IUserActivityService>();
            
            if (userActivityService != null)
            {
                var ipAddress = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString() ?? "";
                var userAgent = Context.GetHttpContext()?.Request.Headers["User-Agent"].ToString() ?? "";
                
                Console.WriteLine($"[UserActivityHub] User {username} connected via SignalR");
                await userActivityService.UserConnectedAsync(userId, username, ipAddress, userAgent);
            }
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = Context.UserIdentifier;
        var username = Context.User?.Identity?.Name;
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "OnlineUsers");
            
            // Get UserActivityService from DI
            var serviceProvider = Context.GetHttpContext()?.RequestServices;
            var userActivityService = serviceProvider?.GetService<IUserActivityService>();
            
            if (userActivityService != null)
            {
                Console.WriteLine($"[UserActivityHub] User {username} disconnected via SignalR");
                await userActivityService.UserDisconnectedAsync(userId);
            }
        }
        await base.OnDisconnectedAsync(exception);
    }
}