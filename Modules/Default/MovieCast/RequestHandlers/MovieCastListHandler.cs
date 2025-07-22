using Serenity.Services;
using MyRequest = Serenity.Services.ListRequest;
using MyResponse = Serenity.Services.ListResponse<SerenityProjem.Default.MovieCastRow>;
using MyRow = SerenityProjem.Default.MovieCastRow;

namespace SerenityProjem.Default;

public interface IMovieCastListHandler : IListHandler<MyRow, MyRequest, MyResponse> { }

public class MovieCastListHandler : ListRequestHandler<MyRow, MyRequest, MyResponse>, IMovieCastListHandler
{
    public MovieCastListHandler(IRequestContext context)
            : base(context)
    {
    }
}