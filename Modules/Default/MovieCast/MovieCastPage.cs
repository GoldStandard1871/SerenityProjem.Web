using Microsoft.AspNetCore.Mvc;
using Serenity.Web;

namespace SerenityProjem.Default.Pages;

[PageAuthorize(typeof(MovieCastRow))]
public class MovieCastPage : Controller
{
    [Route("Default/MovieCast")]
    public ActionResult Index()
    {
        return this.GridPage<MovieCastRow>("@/Default/MovieCast/MovieCastPage");
    }
}