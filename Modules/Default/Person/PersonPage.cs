using Microsoft.AspNetCore.Mvc;
using Serenity.Web;

namespace SerenityProjem.Default.Pages;

[PageAuthorize(typeof(PersonRow))]
public class PersonPage : Controller
{
    [Route("Default/Person")]
    public ActionResult Index()
    {
        return this.GridPage<PersonRow>("@/Default/Person/PersonPage");
    }
}