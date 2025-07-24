using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SerenityProjem.Membership.Pages;

[Route("Account/[action]")]
public partial class AccountPage : Controller
{
    protected ITwoLevelCache Cache { get; }
    protected ITextLocalizer Localizer { get; }
    public AccountPage(ITwoLevelCache cache, ITextLocalizer localizer)
    {
        Localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        Cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    [HttpGet]
    public ActionResult Login(int? denied, string activated, string returnUrl)
    {
        if (denied == 1)
            return View(MVC.Views.Errors.AccessDenied,
                ("~/Account/Login?returnUrl=" + Uri.EscapeDataString(returnUrl)));

        ViewData["Activated"] = activated;
        ViewData["HideLeftNavigation"] = true;
        return View(MVC.Views.Membership.Account.Login.LoginPage);
    }

    [HttpGet]
    public ActionResult AccessDenied(string returnURL)
    {
        ViewData["HideLeftNavigation"] = !User.IsLoggedIn();

        return View(MVC.Views.Errors.AccessDenied, (object)returnURL);
    }

    [HttpPost, JsonRequest]
    public Result<ServiceResponse> Login(LoginRequest request,
        [FromServices] IUserPasswordValidator passwordValidator,
        [FromServices] IUserClaimCreator userClaimCreator,
        [FromServices] Administration.IUserActivityService userActivityService)
    {

        return this.ExecuteMethod(() =>
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(request.Username))
                throw new ArgumentNullException(nameof(request.Username));

            if (passwordValidator is null)
                throw new ArgumentNullException(nameof(passwordValidator));

            if (userClaimCreator is null)
                throw new ArgumentNullException(nameof(userClaimCreator));

            var username = request.Username;
            var result = passwordValidator.Validate(ref username, request.Password);
            if (result == PasswordValidationResult.Valid)
            {
                var principal = userClaimCreator.CreatePrincipal(username, authType: "Password");
                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal).GetAwaiter().GetResult();
                
                // User activity tracking
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? username;
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                
                Console.WriteLine($"[UserActivity] Login attempt - UserId: {userId}, Username: {username}, IP: {ipAddress}");
                
                _ = Task.Run(async () => 
                {
                    try
                    {
                        Console.WriteLine($"[UserActivity] Calling UserConnectedAsync for {username}");
                        await userActivityService.UserConnectedAsync(userId, username, ipAddress, userAgent);
                        Console.WriteLine($"[UserActivity] UserConnectedAsync completed for {username}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[UserActivity] Error tracking user activity: {ex.Message}");
                        Console.WriteLine($"[UserActivity] Stack trace: {ex.StackTrace}");
                    }
                });
                
                return new ServiceResponse();
            }

            if (result == PasswordValidationResult.InactiveUser)
            {
                throw new ValidationError("InactivatedAccount", MembershipValidationTexts.AuthenticationError.ToString(Localizer));
            }

            throw new ValidationError("AuthenticationError", MembershipValidationTexts.AuthenticationError.ToString(Localizer));
        });
    }

    private ActionResult Error(string message)
    {
        return View(MVC.Views.Errors.ValidationError, new ValidationError(message));
    }

    public string KeepAlive()
    {
        return "OK";
    }

    public ActionResult Signout([FromServices] Administration.IUserActivityService userActivityService)
    {
        // Track user disconnect before signing out
        var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            _ = Task.Run(async () => 
            {
                try
                {
                    await userActivityService.UserDisconnectedAsync(userId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error tracking user disconnect: {ex.Message}");
                }
            });
        }

        HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.RequestServices.GetService<IElevationHandler>()?.DeleteToken();
        return new RedirectResult("~/");
    }
}