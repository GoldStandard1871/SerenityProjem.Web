using Serenity.Services;
using MyRequest = Serenity.Services.ListRequest;
using MyResponse = Serenity.Services.ListResponse<SerenityProjem.Default.MovieRow>;
using MyRow = SerenityProjem.Default.MovieRow;

namespace SerenityProjem.Default;

public interface IMovieListHandler : IListHandler<MyRow, MyRequest, MyResponse> { }

public class MovieListHandler : ListRequestHandler<MyRow, MyRequest, MyResponse>, IMovieListHandler
{
    public MovieListHandler(IRequestContext context)
            : base(context)
    {
    }
}