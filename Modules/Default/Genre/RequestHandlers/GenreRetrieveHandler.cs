using Serenity.Services;
using MyRequest = Serenity.Services.RetrieveRequest;
using MyResponse = Serenity.Services.RetrieveResponse<SerenityProjem.Default.GenreRow>;
using MyRow = SerenityProjem.Default.GenreRow;

namespace SerenityProjem.Default;

public interface IGenreRetrieveHandler : IRetrieveHandler<MyRow, MyRequest, MyResponse> { }

public class GenreRetrieveHandler : RetrieveRequestHandler<MyRow, MyRequest, MyResponse>, IGenreRetrieveHandler
{
    public GenreRetrieveHandler(IRequestContext context)
            : base(context)
    {
    }
}