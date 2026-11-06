using MaturaArbeiten.Domain;
using MaturaArbeiten.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaturaArbeiten.Controllers;

[Authorize(Roles = Rollen.G3SuS)]
public class EintragungController(
    IEintragungService service,
    IBestaetigungService bestaetigungService) : Controller
{
    public IActionResult MeineEintragungen() => View(service.Meine(User.BenutzerId()));

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult WaehlePraesentation(int praesentationId)
    {
        var g3susId = User.BenutzerId();

        switch (service.Eintragen(g3susId, praesentationId))
        {
            case EintragungErgebnis.Abgewiesen abgewiesen:
                TempData["Fehler"] = Meldungen.Fuer(abgewiesen.Grund);
                return RedirectToAction("Index", "Praesentation");

            default:
                return NachDerEintragung(g3susId);
        }
    }

    private IActionResult NachDerEintragung(int g3susId)
    {
        switch (bestaetigungService.Senden(g3susId))
        {
            case Versand.Gesendet:
                TempData["Erfolg"] = "Deine Eintragung wurde gespeichert. Die Bestätigung wurde per E-Mail versendet.";
                return RedirectToAction(nameof(MeineEintragungen));

            case Versand.Fehlgeschlagen:
                TempData["Fehler"] = "Deine Eintragung wurde gespeichert. Die Bestätigung konnte nicht verschickt werden, die Administration kann sie noch einmal senden.";
                return RedirectToAction(nameof(MeineEintragungen));

            default:
                TempData["Erfolg"] = "Deine Eintragung wurde gespeichert.";
                return RedirectToAction("Index", "Praesentation");
        }
    }
}
