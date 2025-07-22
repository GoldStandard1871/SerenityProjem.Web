using Serenity.Services;
using MyRequest = Serenity.Services.ListRequest;
using MyResponse = Serenity.Services.ListResponse<SerenityProjem.Default.PersonRow>;
using MyRow = SerenityProjem.Default.PersonRow;

namespace SerenityProjem.Default;

public interface IPersonListHandler : IListHandler<MyRow, MyRequest, MyResponse> { }

public class PersonListHandler : ListRequestHandler<MyRow, MyRequest, MyResponse>, IPersonListHandler
{
    public PersonListHandler(IRequestContext context)
            : base(context)
    {
    }
}