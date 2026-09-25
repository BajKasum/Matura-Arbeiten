using Microsoft.AspNetCore.Mvc;

namespace MaturaArbeiten.Controllers;

public class EintragungController : Controller
{
    public IActionResult MeineEintragungen()
    {
        return View();
    }
}