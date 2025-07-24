using Microsoft.AspNetCore.Mvc;
using Serenity.Web;

namespace SerenityProjem.Administration.Pages;

[PageAuthorize(typeof(UserActivityHistoryRow))]
public class UserActivityHistoryPage : Controller
{
    [Route("Administration/UserActivityHistory")]
    public ActionResult Index()
    {
        return this.GridPage("@/Administration/UserActivityHistory/UserActivityHistoryIndex",
            UserActivityHistoryRow.Fields.PageTitle());
    }
}