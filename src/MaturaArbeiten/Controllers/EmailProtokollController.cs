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

        if (gesendet + fehlgeschlagen == 0)
        {
            TempData["Fehler"] = "Es gibt keine fehlgeschlagenen E-Mails.";
        }
        else if (fehlgeschlagen > 0)
        {
            TempData["Fehler"] = $"{fehlgeschlagen} E-Mails konnten nicht verschickt werden. {gesendet} wurden verschickt.";
        }
        else
        {
            TempData["Erfolg"] = $"{gesendet} E-Mails wurden verschickt.";
        }

        return RedirectToAction("Index", "Admin");
    }
}
