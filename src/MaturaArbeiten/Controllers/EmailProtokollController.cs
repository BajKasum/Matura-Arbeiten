using MaturaArbeiten.Domain;
using MaturaArbeiten.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaturaArbeiten.Controllers;

[Authorize(Roles = Rollen.Administration)]
public class EmailProtokollController(IEmailProtokollService service) : Controller
{
    public IActionResult Index() => View(service.Fehlgeschlagene());

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ErneutSenden(int id)
    {
        if (service.ErneutSenden(id))
        {
            TempData["Erfolg"] = "Die E-Mail wurde verschickt.";
        }
        else
        {
            TempData["Fehler"] = "Die E-Mail konnte nicht verschickt werden.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult AlleErneutSenden()
    {
        var (gesendet, fehlgeschlagen) = service.AlleErneutSenden();
        var (erfolg, text) = Meldungen.Versandergebnis(
            gesendet,
            fehlgeschlagen,
            "E-Mails",
            "Es gibt keine fehlgeschlagenen E-Mails.");

        TempData[erfolg ? "Erfolg" : "Fehler"] = text;

        return RedirectToAction("Index", "Admin");
    }
}
