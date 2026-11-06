using MaturaArbeiten.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaturaArbeiten.Controllers;

[Authorize(Roles = Rollen.Administration)]
public class AdminController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}