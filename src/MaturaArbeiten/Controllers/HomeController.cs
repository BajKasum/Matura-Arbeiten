using System.Diagnostics;
using MaturaArbeiten.Domain;
using MaturaArbeiten.Models;
using Microsoft.AspNetCore.Mvc;

namespace MaturaArbeiten.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() =>
        User.IsInRole(Rollen.Administration) ? RedirectToAction("Index", "Admin")
        : User.IsInRole(Rollen.G3SuS) ? RedirectToAction("Index", "Praesentation")
        : RedirectToAction("Anmelden", "Account");

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}