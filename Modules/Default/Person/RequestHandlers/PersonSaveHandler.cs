using Serenity.Services;
using MyRequest = Serenity.Services.SaveRequest<SerenityProjem.Default.PersonRow>;
using MyResponse = Serenity.Services.SaveResponse;
using MyRow = SerenityProjem.Default.PersonRow;

namespace SerenityProjem.Default;

public interface IPersonSaveHandler : ISaveHandler<MyRow, MyRequest, MyResponse> { }

public class PersonSaveHandler : SaveRequestHandler<MyRow, MyRequest, MyResponse>, IPersonSaveHandler
{
    public PersonSaveHandler(IRequestContext context)
            : base(context)
    {
    }
}