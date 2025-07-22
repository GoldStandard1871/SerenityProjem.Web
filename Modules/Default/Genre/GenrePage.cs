using Microsoft.AspNetCore.Mvc;
using Serenity.Web;

namespace SerenityProjem.Default.Pages;

[PageAuthorize(typeof(GenreRow))]
public class GenrePage : Controller
{
    [Route("Default/Genre")]
    public ActionResult Index()
    {
        return this.GridPage<GenreRow>("@/Default/Genre/GenrePage");
    }
}