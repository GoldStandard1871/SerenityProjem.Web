using Serenity.Services;
using Serenity.Data;
using Microsoft.Extensions.Logging;
using System.Linq;
using MyRequest = Serenity.Services.SaveRequest<SerenityProjem.Default.MovieCastRow>;
using MyResponse = Serenity.Services.SaveResponse;
using MyRow = SerenityProjem.Default.MovieCastRow;

namespace SerenityProjem.Default;

public interface IMovieCastSaveHandler : ISaveHandler<MyRow, MyRequest, MyResponse> { }

public class MovieCastSaveHandler : SaveRequestHandler<MyRow, MyRequest, MyResponse>, IMovieCastSaveHandler
{
    public MovieCastSaveHandler(IRequestContext context)
            : base(context)
    {
    }

    protected override void ValidateRequest()
    {
        base.ValidateRequest();

        if (Row.MovieId == null || Row.MovieId <= 0)
            throw new ValidationError("Movie selection is required!", "MovieId");

        if (Row.PersonId == null || Row.PersonId <= 0)
            throw new ValidationError("Person selection is required!", "PersonId");

        if (string.IsNullOrWhiteSpace(Row.Character))
            throw new ValidationError("Character name is required!", "Character");
    }

    protected override void BeforeSave()
    {
        base.BeforeSave();

        // Aynı Movie ve Person kombinasyonu kontrolü - basit SQL kullan
        if (IsUpdate)
            return;

        try
        {
            var count = Connection.Query<int>(@"
                SELECT COUNT(*) 
                FROM MovieCast 
                WHERE MovieId = @movieId AND PersonId = @personId", 
                new { 
                    movieId = Row.MovieId.Value,
                    personId = Row.PersonId.Value 
                }).FirstOrDefault();
            
            if (count > 0)
            {
                throw new ValidationError("This person is already cast in this movie!", "PersonId");
            }
        }
        catch (System.Exception ex)
        {
            // SQL hatası durumunda sadece log at, işleme devam et
            System.Diagnostics.Debug.WriteLine($"MovieCast duplicate check error: {ex.Message}");
        }
    }

    protected override void AfterSave()
    {
        base.AfterSave();
        
        // Log the successful save - Console log olarak
        Console.WriteLine($"[MovieCast] Saved: Movie {Row.MovieId}, Person {Row.PersonId}, Character '{Row.Character}'");
    }
}