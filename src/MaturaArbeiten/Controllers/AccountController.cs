using Microsoft.AspNetCore.Mvc;

namespace MaturaArbeiten.Controllers;

public class AccountController : Controller
{
    public IActionResult Anmelden()
    {
        return View();
    }
}