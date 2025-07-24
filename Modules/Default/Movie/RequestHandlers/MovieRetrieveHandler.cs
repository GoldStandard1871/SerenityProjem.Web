using Serenity.Data;
using Serenity.Services;
using System.Collections.Generic;
using System.Linq;
using MyRequest = Serenity.Services.RetrieveRequest;
using MyResponse = Serenity.Services.RetrieveResponse<SerenityProjem.Default.MovieRow>;
using MyRow = SerenityProjem.Default.MovieRow;

namespace SerenityProjem.Default;

public interface IMovieRetrieveHandler : IRetrieveHandler<MyRow, MyRequest, MyResponse> { }

public class MovieRetrieveHandler : RetrieveRequestHandler<MyRow, MyRequest, MyResponse>, IMovieRetrieveHandler
{
    public MovieRetrieveHandler(IRequestContext context)
            : base(context)
    {
    }

    protected override void OnReturn()
    {
        base.OnReturn();

        // CastList yükleme - gerçek verileri yükle
        if (Row.MovieId.HasValue)
        {
            try
            {
                // Basit SQL sorgusu ile MovieCast verilerini yükle
                var castData = Connection.Query<dynamic>(@"
                    SELECT 
                        mc.MovieCastId,
                        mc.MovieId, 
                        mc.PersonId, 
                        mc.Character,
                        m.Title as MovieTitle,
                        CONCAT(p.FirstName, ' ', p.LastName) as PersonFullName
                    FROM MovieCast mc
                    LEFT JOIN Movie m ON mc.MovieId = m.MovieId
                    LEFT JOIN Person p ON mc.PersonId = p.PersonId  
                    WHERE mc.MovieId = @movieId", 
                    new { movieId = Row.MovieId.Value });

                // Dynamic results'u MovieCastRow'a dönüştür
                Row.CastList = castData.Select(item => new MovieCastRow
                {
                    MovieCastId = item.MovieCastId,
                    MovieId = item.MovieId,
                    PersonId = item.PersonId,
                    Character = item.Character,
                    MovieTitle = item.MovieTitle,
                    PersonFullName = item.PersonFullName
                }).ToList();

                System.Diagnostics.Debug.WriteLine($"MovieCast loaded {Row.CastList.Count} records for Movie {Row.MovieId}");
            }
            catch (System.Exception ex)
            {
                // Hata durumunda boş liste döndür
                Row.CastList = new List<MovieCastRow>();
                System.Diagnostics.Debug.WriteLine($"MovieCast retrieve error: {ex.Message}");
            }
        }
    }
}