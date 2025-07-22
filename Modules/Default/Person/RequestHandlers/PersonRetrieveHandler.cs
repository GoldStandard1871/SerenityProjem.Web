using Serenity.Services;
using MyRequest = Serenity.Services.RetrieveRequest;
using MyResponse = Serenity.Services.RetrieveResponse<SerenityProjem.Default.PersonRow>;
using MyRow = SerenityProjem.Default.PersonRow;

namespace SerenityProjem.Default;

public interface IPersonRetrieveHandler : IRetrieveHandler<MyRow, MyRequest, MyResponse> { }

public class PersonRetrieveHandler : RetrieveRequestHandler<MyRow, MyRequest, MyResponse>, IPersonRetrieveHandler
{
    public PersonRetrieveHandler(IRequestContext context)
            : base(context)
    {
    }
}