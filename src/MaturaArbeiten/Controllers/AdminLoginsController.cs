using MaturaArbeiten.Domain;
using MaturaArbeiten.Services;
using MaturaArbeiten.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaturaArbeiten.Controllers;

[Authorize(Roles = Rollen.Administration)]
public class AdminLoginsController(
    IAdminLoginService service,
    IZugangsdatenService zugangsdatenService) : Controller
{
    public IActionResult Index() => View(service.Liste());

    [HttpGet]
    public IActionResult Neu() => View("Formular", new PersonFormular());

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Neu(PersonFormular eingabe)
    {
        if (!ModelState.IsValid)
        {
            return View("Formular", eingabe);
        }

        var (id, fehler) = service.Anlegen(eingabe);

        if (id is null)
        {
            ModelState.AddModelError(nameof(PersonFormular.Email), fehler!);
            return View("Formular", eingabe);
        }

        if (zugangsdatenService.AnAdministrationslogin(id.Value))
        {
            TempData["Erfolg"] = "Das Administrationslogin wurde angelegt. Die Zugangsdaten wurden verschickt.";
        }
        else
        {
            TempData["Fehler"] = "Das Administrationslogin wurde angelegt, die Zugangsdaten konnten aber nicht verschickt werden. Bitte versuche es mit \"Neue Zugangsdaten\" noch einmal.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Bearbeiten(int id) =>
        service.Finden(id) is { } formular ? View("Formular", formular) : NotFound();

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Bearbeiten(int id, PersonFormular eingabe)
    {
        if (!ModelState.IsValid)
        {
            return View("Formular", eingabe);
        }

        var fehler = service.Aendern(id, eingabe);

        if (fehler is not null)
        {
            ModelState.AddModelError(nameof(PersonFormular.Email), fehler);
            return View("Formular", eingabe);
        }

        TempData["Erfolg"] = "Die Änderung wurde gespeichert.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ZugangsdatenSenden(int id)
    {
        if (zugangsdatenService.AnAdministrationslogin(id))
        {
            TempData["Erfolg"] = "Neue Zugangsdaten wurden verschickt.";
        }
        else
        {
            TempData["Fehler"] = "Die Zugangsdaten konnten nicht verschickt werden.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Loeschen(int id)
    {
        var fehler = service.Loeschen(id);

        if (fehler is null)
        {
            TempData["Erfolg"] = "Das Administrationslogin wurde gelöscht.";
        }
        else
        {
            TempData["Fehler"] = fehler;
        }

        return RedirectToAction(nameof(Index));
    }
}
