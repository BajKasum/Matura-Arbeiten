using MaturaArbeiten.Domain;
using MaturaArbeiten.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaturaArbeiten.Controllers;

[Authorize(Roles = Rollen.G3SuS)]
public class PraesentationController(IPraesentationService service) : Controller
{
    public IActionResult Index() => View(service.Liste(User.BenutzerId()));
}