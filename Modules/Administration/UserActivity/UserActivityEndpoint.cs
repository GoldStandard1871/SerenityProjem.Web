using Microsoft.AspNetCore.Mvc;
using Serenity;
using Serenity.Data;
using Serenity.Reporting;
using Serenity.Services;
using Serenity.Web;
using MyRequest = Serenity.Services.ListRequest;
using MyResponse = Serenity.Services.ListResponse<SerenityProjem.Administration.OnlineUserInfo>;
using MyRow = SerenityProjem.Administration.UserRow;

namespace SerenityProjem.Administration.Endpoints;

[Route("Services/Administration/UserActivity/[action]")]
[ConnectionKey(typeof(MyRow)), ServiceAuthorize(typeof(MyRow))]
public class UserActivityEndpoint : ServiceEndpoint
{
    private readonly IUserActivityService _userActivityService;

    public UserActivityEndpoint(IUserActivityService userActivityService)
    {
        _userActivityService = userActivityService;
    }

    [HttpPost, ReadPermission(typeof(MyRow))]
    public MyResponse GetOnlineUsers(MyRequest request)
    {
        var onlineUsers = _userActivityService.GetOnlineUsers();

        return new MyResponse
        {
            Entities = onlineUsers,
            TotalCount = onlineUsers.Count
        };
    }

    [HttpPost, ReadPermission(typeof(MyRow))]
    public Result<OnlineUsersCountResponse> GetOnlineUsersCount()
    {
        return this.ExecuteMethod(() =>
        {
            var count = _userActivityService.GetOnlineUsersCount();
            return new OnlineUsersCountResponse { Count = count };
        });
    }
}

public class OnlineUsersCountResponse : ServiceResponse
{
    public int Count { get; set; }
}