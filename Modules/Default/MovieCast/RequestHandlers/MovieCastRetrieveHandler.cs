using Serenity.Services;
using MyRequest = Serenity.Services.RetrieveRequest;
using MyResponse = Serenity.Services.RetrieveResponse<SerenityProjem.Default.MovieCastRow>;
using MyRow = SerenityProjem.Default.MovieCastRow;

namespace SerenityProjem.Default;

public interface IMovieCastRetrieveHandler : IRetrieveHandler<MyRow, MyRequest, MyResponse> { }

public class MovieCastRetrieveHandler : RetrieveRequestHandler<MyRow, MyRequest, MyResponse>, IMovieCastRetrieveHandler
{
    public MovieCastRetrieveHandler(IRequestContext context)
            : base(context)
    {
    }
}