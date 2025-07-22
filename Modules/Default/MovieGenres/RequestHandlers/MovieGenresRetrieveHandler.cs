using Serenity.Services;
using MyRequest = Serenity.Services.RetrieveRequest;
using MyResponse = Serenity.Services.RetrieveResponse<SerenityProjem.Default.MovieGenresRow>;
using MyRow = SerenityProjem.Default.MovieGenresRow;

namespace SerenityProjem.Default;

public interface IMovieGenresRetrieveHandler : IRetrieveHandler<MyRow, MyRequest, MyResponse> { }

public class MovieGenresRetrieveHandler : RetrieveRequestHandler<MyRow, MyRequest, MyResponse>, IMovieGenresRetrieveHandler
{
    public MovieGenresRetrieveHandler(IRequestContext context)
            : base(context)
    {
    }
}