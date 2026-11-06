using System.Security.Claims;
using MaturaArbeiten.Domain;
using MaturaArbeiten.Services;
using MaturaArbeiten.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace MaturaArbeiten.Controllers;

public class AccountController(IAnmeldeService anmeldeService, TimeProvider zeit) : Controller
{
    [HttpGet]
    public IActionResult Anmelden() =>
        User.Identity?.IsAuthenticated == true
            ? RedirectToAction("Index", "Home")
            : View(new AnmeldenViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Anmelden(AnmeldenViewModel eingabe)
    {
        if (!ModelState.IsValid)
        {
            return View(eingabe);
        }

        switch (anmeldeService.Anmelden(eingabe.Email.Trim(), eingabe.Passwort))
        {
            case AnmeldeErgebnis.Erfolg erfolg:
                var identitaet = new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, erfolg.Id.ToString()),
                        new Claim(ClaimTypes.Name, erfolg.Name),
                        new Claim(ClaimTypes.Role, erfolg.Rolle)
                    },
                    CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identitaet));

                return RedirectToAction("Index", "Home");

            case AnmeldeErgebnis.Gesperrt gesperrt:
                ModelState.AddModelError(
                    string.Empty,
                    Meldungen.Gesperrt(gesperrt.Bis, zeit.GetUtcNow().UtcDateTime));
                return View(eingabe);

            default:
                ModelState.AddModelError(string.Empty, Meldungen.AnmeldungFalsch);
                return View(eingabe);
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Abmelden()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Anmelden");
    }
}