using MyRow = SerenityProjem.Administration.UserRow;
using MyRequest = SerenityProjem.Administration.UserListRequest;
using MyResponse = Serenity.Services.ListResponse<SerenityProjem.Administration.UserRow>;

namespace SerenityProjem.Administration;

public interface IUserListHandler : IListHandler<MyRow, MyRequest, MyResponse> { }

public class UserListHandler : ListRequestHandler<MyRow, MyRequest, MyResponse>, IUserListHandler
{
    public UserListHandler(IRequestContext context)
         : base(context)
    {
    }
}