using MaturaArbeiten.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaturaArbeiten.Controllers;

[Authorize(Roles = Rollen.G3SuS)]
public class PraesentationController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}