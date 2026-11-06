using MaturaArbeiten.Domain;
using MaturaArbeiten.Services;
using MaturaArbeiten.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaturaArbeiten.Controllers;

[Authorize(Roles = Rollen.Administration)]
public class AdminEintragungenController(IEintragungService service) : Controller
{
    public IActionResult Index() => View(service.Alle());

    [HttpGet]
    public IActionResult Neu() => View("Formular", service.Formular(null));

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Neu(EintragungFormular eingabe)
    {
        if (!ModelState.IsValid)
        {
            return View("Formular", service.Formular(null)! with { Eingabe = eingabe });
        }

        switch (service.EintragenDurchAdministration(eingabe.G3susId, eingabe.PraesentationId))
        {
            case EintragungErgebnis.Abgewiesen abgewiesen:
                ModelState.AddModelError(string.Empty, Meldungen.FuerAdministration(abgewiesen.Grund));
                return View("Formular", service.Formular(null)! with { Eingabe = eingabe });

            default:
                TempData["Erfolg"] = "Die Eintragung wurde gespeichert.";
                return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public IActionResult Aendern(int id) =>
        service.Formular(id) is { } formular ? View("Formular", formular) : NotFound();

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Aendern(int id, EintragungFormular eingabe)
    {
        var formular = service.Formular(id);

        if (formular is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View("Formular", formular with { Eingabe = eingabe });
        }

        switch (service.Aendern(id, eingabe.PraesentationId))
        {
            case EintragungErgebnis.Abgewiesen abgewiesen:
                ModelState.AddModelError(string.Empty, Meldungen.FuerAdministration(abgewiesen.Grund));
                return View("Formular", formular with { Eingabe = eingabe });

            default:
                TempData["Erfolg"] = "Die Änderung wurde gespeichert.";
                return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Loeschen(int id)
    {
        if (service.Loeschen(id))
        {
            TempData["Erfolg"] = "Die Eintragung wurde gelöscht. Der Platz ist wieder frei.";
        }
        else
        {
            TempData["Fehler"] = "Diese Eintragung gibt es nicht mehr.";
        }

        return RedirectToAction(nameof(Index));
    }
}
