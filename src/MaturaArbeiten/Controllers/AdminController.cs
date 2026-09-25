using Microsoft.AspNetCore.Mvc;

namespace MaturaArbeiten.Controllers;

public class AdminController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}