using Serenity.Services;
using MyRequest = Serenity.Services.SaveRequest<SerenityProjem.Default.MovieRow>;
using MyResponse = Serenity.Services.SaveResponse;
using MyRow = SerenityProjem.Default.MovieRow;

namespace SerenityProjem.Default;

public interface IMovieSaveHandler : ISaveHandler<MyRow, MyRequest, MyResponse> { }

public class MovieSaveHandler : SaveRequestHandler<MyRow, MyRequest, MyResponse>, IMovieSaveHandler
{
    public MovieSaveHandler(IRequestContext context)
            : base(context)
    {
    }
}