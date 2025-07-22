using Serenity.Services;
using MyRequest = Serenity.Services.SaveRequest<SerenityProjem.Default.MovieGenresRow>;
using MyResponse = Serenity.Services.SaveResponse;
using MyRow = SerenityProjem.Default.MovieGenresRow;

namespace SerenityProjem.Default;

public interface IMovieGenresSaveHandler : ISaveHandler<MyRow, MyRequest, MyResponse> { }

public class MovieGenresSaveHandler : SaveRequestHandler<MyRow, MyRequest, MyResponse>, IMovieGenresSaveHandler
{
    public MovieGenresSaveHandler(IRequestContext context)
            : base(context)
    {
    }
}