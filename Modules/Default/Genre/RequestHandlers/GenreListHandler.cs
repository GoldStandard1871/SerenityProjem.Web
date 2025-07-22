using Serenity.Services;
using MyRequest = Serenity.Services.ListRequest;
using MyResponse = Serenity.Services.ListResponse<SerenityProjem.Default.GenreRow>;
using MyRow = SerenityProjem.Default.GenreRow;

namespace SerenityProjem.Default;

public interface IGenreListHandler : IListHandler<MyRow, MyRequest, MyResponse> { }

public class GenreListHandler : ListRequestHandler<MyRow, MyRequest, MyResponse>, IGenreListHandler
{
    public GenreListHandler(IRequestContext context)
            : base(context)
    {
    }
}