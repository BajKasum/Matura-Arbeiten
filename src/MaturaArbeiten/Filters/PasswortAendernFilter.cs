using MaturaArbeiten.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MaturaArbeiten.Filters;

public sealed class PasswortAendernFilter : IAsyncActionFilter
{
    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var controller = context.RouteData.Values["controller"]?.ToString();
        var action = context.RouteData.Values["action"]?.ToString();

        if (context.HttpContext.User.HasClaim(Ansprueche.PasswortAendern, "true") && !IstErlaubt(controller, action))
        {
            context.Result = new RedirectToActionResult("PasswortAendern", "Account", null);
            return Task.CompletedTask;
        }

        return next();
    }

    private static bool IstErlaubt(string? controller, string? action) =>
        (controller, action) is ("Account", "PasswortAendern" or "Abmelden") or ("Home", "Error");
}
