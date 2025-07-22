using Serenity.Services;
using MyRequest = Serenity.Services.SaveRequest<SerenityProjem.Default.GenreRow>;
using MyResponse = Serenity.Services.SaveResponse;
using MyRow = SerenityProjem.Default.GenreRow;

namespace SerenityProjem.Default;

public interface IGenreSaveHandler : ISaveHandler<MyRow, MyRequest, MyResponse> { }

public class GenreSaveHandler : SaveRequestHandler<MyRow, MyRequest, MyResponse>, IGenreSaveHandler
{
    public GenreSaveHandler(IRequestContext context)
            : base(context)
    {
    }
}