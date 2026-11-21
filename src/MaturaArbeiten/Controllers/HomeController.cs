using System.Diagnostics;
using MaturaArbeiten.Domain;
using MaturaArbeiten.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaturaArbeiten.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    public IActionResult Index() =>
        User.IsInRole(Rollen.Administration) ? RedirectToAction("Index", "Admin")
        : User.IsInRole(Rollen.G3SuS) ? RedirectToAction("Index", "Praesentation")
        : RedirectToAction("Anmelden", "Account");

    public IActionResult Status(int code) =>
        code == StatusCodes.Status404NotFound ? View("NichtGefunden") : StatusCode(code);

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new FehlerViewModel { AnfrageId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}