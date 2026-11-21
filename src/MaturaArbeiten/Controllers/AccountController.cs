using System.Security.Claims;
using MaturaArbeiten.Domain;
using MaturaArbeiten.Services;
using MaturaArbeiten.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaturaArbeiten.Controllers;

public class AccountController(
    IAnmeldeService anmeldeService,
    IPasswortService passwortService,
    TimeProvider zeit) : Controller
{
    [AllowAnonymous, HttpGet]
    public IActionResult Anmelden() =>
        User.Identity?.IsAuthenticated == true
            ? RedirectToAction("Index", "Home")
            : View(new AnmeldenViewModel());

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Anmelden(AnmeldenViewModel eingabe)
    {
        if (!ModelState.IsValid)
        {
            return View(eingabe);
        }

        switch (anmeldeService.Anmelden(eingabe.Email.Trim(), eingabe.Passwort))
        {
            case AnmeldeErgebnis.Erfolg erfolg:
                var ansprueche = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, erfolg.Id.ToString()),
                    new(ClaimTypes.Name, erfolg.Name),
                    new(ClaimTypes.Role, erfolg.Rolle)
                };

                if (erfolg.PasswortAendern)
                {
                    ansprueche.Add(new Claim(Ansprueche.PasswortAendern, "true"));
                }

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(new ClaimsIdentity(ansprueche, CookieAuthenticationDefaults.AuthenticationScheme)));

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

    [Authorize, HttpGet]
    public IActionResult KeinZugriff()
    {
        TempData["Fehler"] = User.IsInRole(Rollen.Administration)
            ? "Diese Seite ist nur für G3-SuS."
            : "Diese Seite ist nur für die Administration.";

        return RedirectToAction("Index", "Home");
    }

    [Authorize, HttpGet]
    public IActionResult PasswortAendern() => View(new PasswortAendernFormular());

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PasswortAendern(PasswortAendernFormular eingabe)
    {
        if (!ModelState.IsValid)
        {
            return View(eingabe);
        }

        var regelFehler = Passwoerter.Pruefe(eingabe.Aktuell, eingabe.Neu, eingabe.Wiederholung);

        if (regelFehler is not null)
        {
            ModelState.AddModelError(string.Empty, regelFehler);
            return View(eingabe);
        }

        var fehler = passwortService.Aendern(
            User.FindFirstValue(ClaimTypes.Role)!,
            User.BenutzerId(),
            eingabe.Aktuell,
            eingabe.Neu);

        if (fehler is not null)
        {
            ModelState.AddModelError(string.Empty, fehler);
            return View(eingabe);
        }

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(
                User.Claims.Where(a => a.Type != Ansprueche.PasswortAendern),
                CookieAuthenticationDefaults.AuthenticationScheme)));

        TempData["Erfolg"] = "Dein Passwort wurde geändert.";
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Abmelden()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Anmelden");
    }
}
