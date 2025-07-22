using Serenity.Services;
using MyRequest = Serenity.Services.ListRequest;
using MyResponse = Serenity.Services.ListResponse<SerenityProjem.Default.MovieGenresRow>;
using MyRow = SerenityProjem.Default.MovieGenresRow;

namespace SerenityProjem.Default;

public interface IMovieGenresListHandler : IListHandler<MyRow, MyRequest, MyResponse> { }

public class MovieGenresListHandler : ListRequestHandler<MyRow, MyRequest, MyResponse>, IMovieGenresListHandler
{
    public MovieGenresListHandler(IRequestContext context)
            : base(context)
    {
    }
}