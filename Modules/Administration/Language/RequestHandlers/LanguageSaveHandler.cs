using MyRow = SerenityProjem.Administration.LanguageRow;
using MyRequest = Serenity.Services.SaveRequest<SerenityProjem.Administration.LanguageRow>;
using MyResponse = Serenity.Services.SaveResponse;


namespace SerenityProjem.Administration;

public interface ILanguageSaveHandler : ISaveHandler<MyRow, MyRequest, MyResponse> { }
public class LanguageSaveHandler : SaveRequestHandler<MyRow, MyRequest, MyResponse>, ILanguageSaveHandler
{
    public LanguageSaveHandler(IRequestContext context)
         : base(context)
    {
    }
}