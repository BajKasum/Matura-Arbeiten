using MaturaArbeiten.Domain;
using MaturaArbeiten.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaturaArbeiten.Controllers;

[Authorize(Roles = Rollen.G3SuS)]
public class EintragungController(IEintragungService service) : Controller
{
    public IActionResult MeineEintragungen() => View(service.Meine(User.BenutzerId()));

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult WaehlePraesentation(int praesentationId)
    {
        switch (service.Eintragen(User.BenutzerId(), praesentationId))
        {
            case EintragungErgebnis.Erfolg:
                TempData["Erfolg"] = "Deine Eintragung wurde gespeichert.";
                break;

            case EintragungErgebnis.Abgewiesen abgewiesen:
                TempData["Fehler"] = Meldungen.Fuer(abgewiesen.Grund);
                break;
        }

        return RedirectToAction("Index", "Praesentation");
    }
}