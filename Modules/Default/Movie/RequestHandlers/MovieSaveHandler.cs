using Serenity.Data;
using Serenity.Services;
using System.Collections.Generic;
using MyRequest = Serenity.Services.SaveRequest<SerenityProjem.Default.MovieRow>;
using MyResponse = Serenity.Services.SaveResponse;
using MyRow = SerenityProjem.Default.MovieRow;

namespace SerenityProjem.Default;

public interface IMovieSaveHandler : ISaveHandler<MyRow, MyRequest, MyResponse> { }

public class MovieSaveHandler : SaveRequestHandler<MyRow, MyRequest, MyResponse>, IMovieSaveHandler
{
    public MovieSaveHandler(IRequestContext context)
            : base(context)
    {
    }

    protected override void AfterSave()
    {
        base.AfterSave();

        // CastList işlemi - gerçek kaydetme
        if (Row.CastList != null && Row.MovieId.HasValue)
        {
            var movieId = Row.MovieId.Value;
            
            try
            {
                // Önce mevcut MovieCast kayıtlarını sil
                UnitOfWork.Connection.Execute(
                    "DELETE FROM MovieCast WHERE MovieId = @movieId", 
                    new { movieId = movieId });

                // Yeni kayıtları ekle
                foreach (var item in Row.CastList)
                {
                    if (item.PersonId.HasValue && !string.IsNullOrWhiteSpace(item.Character))
                    {
                        UnitOfWork.Connection.Execute(@"
                            INSERT INTO MovieCast (MovieId, PersonId, Character) 
                            VALUES (@movieId, @personId, @character)", 
                            new { 
                                movieId = movieId,
                                personId = item.PersonId.Value,
                                character = item.Character.Trim()
                            });
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"MovieCast saved {Row.CastList.Count} records for Movie {movieId}");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MovieCast save error: {ex.Message}");
                // Hata durumunda exception fırlat ki kullanıcı görsün
                throw new System.Exception($"MovieCast save failed: {ex.Message}", ex);
            }
        }
    }
}