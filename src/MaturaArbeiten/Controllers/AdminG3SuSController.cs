using MaturaArbeiten.Domain;
using MaturaArbeiten.Services;
using MaturaArbeiten.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaturaArbeiten.Controllers;

[Authorize(Roles = Rollen.Administration)]
public class AdminG3SuSController(IG3SuSVerwaltungService service) : Controller
{
    public IActionResult Index() => View(service.Liste());

    [HttpGet]
    public IActionResult Neu() => View("Formular", new G3SuSFormular());

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Neu(G3SuSFormular eingabe)
    {
        if (!ModelState.IsValid)
        {
            return View("Formular", eingabe);
        }

        var fehler = service.Anlegen(eingabe);

        if (fehler is not null)
        {
            ModelState.AddModelError(nameof(G3SuSFormular.Email), fehler);
            return View("Formular", eingabe);
        }

        TempData["Erfolg"] = "Der G3-SuS wurde angelegt.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Bearbeiten(int id) =>
        service.Finden(id) is { } formular ? View("Formular", formular) : NotFound();

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Bearbeiten(int id, G3SuSFormular eingabe)
    {
        if (!ModelState.IsValid)
        {
            return View("Formular", eingabe);
        }

        var fehler = service.Aendern(id, eingabe);

        if (fehler is not null)
        {
            ModelState.AddModelError(nameof(G3SuSFormular.Email), fehler);
            return View("Formular", eingabe);
        }

        TempData["Erfolg"] = "Die Änderung wurde gespeichert.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Loeschen(int id)
    {
        if (service.Loeschen(id))
        {
            TempData["Erfolg"] = "Der G3-SuS und seine Eintragungen wurden gelöscht.";
        }
        else
        {
            TempData["Fehler"] = "Diesen G3-SuS gibt es nicht mehr.";
        }

        return RedirectToAction(nameof(Index));
    }
}
