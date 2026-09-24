using Microsoft.AspNetCore.Mvc;

namespace MaturaArbeiten.Controllers;

public class PraesentationController : Controller
{
    public IActionResult index()
    {
        return View();
    }
}