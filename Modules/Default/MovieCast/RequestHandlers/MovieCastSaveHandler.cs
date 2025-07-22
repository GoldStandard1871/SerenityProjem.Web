using Serenity.Services;
using MyRequest = Serenity.Services.SaveRequest<SerenityProjem.Default.MovieCastRow>;
using MyResponse = Serenity.Services.SaveResponse;
using MyRow = SerenityProjem.Default.MovieCastRow;

namespace SerenityProjem.Default;

public interface IMovieCastSaveHandler : ISaveHandler<MyRow, MyRequest, MyResponse> { }

public class MovieCastSaveHandler : SaveRequestHandler<MyRow, MyRequest, MyResponse>, IMovieCastSaveHandler
{
    public MovieCastSaveHandler(IRequestContext context)
            : base(context)
    {
    }
}