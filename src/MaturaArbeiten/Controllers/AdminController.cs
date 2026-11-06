using MaturaArbeiten.Domain;
using MaturaArbeiten.Services;
using MaturaArbeiten.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaturaArbeiten.Controllers;

[Authorize(Roles = Rollen.Administration)]
public class AdminController(
    IAdministrationService administrationService,
    IImportService importService,
    IZugangsdatenService zugangsdatenService) : Controller
{
    private const long MaxDateigroesse = 1_000_000;
    private const string Bestaetigungswort = "ZURÜCKSETZEN";

    public IActionResult Index() => View(administrationService.Uebersicht());

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult PhaseUmschalten()
    {
        TempData["Erfolg"] = administrationService.PhaseUmschalten()
            ? "Die Eintragungsphase ist jetzt offen."
            : "Die Eintragungsphase ist jetzt geschlossen.";

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Zuruecksetzen() => View(administrationService.Vorschau());

    public IActionResult ZuruecksetzenBestaetigen() => View(administrationService.Vorschau());

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ZuruecksetzenAusfuehren(string? bestaetigung)
    {
        if (!string.Equals(bestaetigung?.Trim(), Bestaetigungswort, StringComparison.Ordinal))
        {
            TempData["Fehler"] = $"Bitte gib zur Bestätigung das Wort {Bestaetigungswort} genau so ein.";
            return RedirectToAction(nameof(ZuruecksetzenBestaetigen));
        }

        administrationService.Zuruecksetzen();
        TempData["Erfolg"] = "Das Schuljahr wurde zurückgesetzt. Die Eintragungsphase ist wieder offen.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ImportPraesentationen(IFormFile? datei) =>
        Importiere(datei, importService.PraesentationenLaden);

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ImportG3SuS(IFormFile? datei) =>
        Importiere(datei, importService.G3SuSLaden);

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ZugangsdatenVersenden()
    {
        var (gesendet, fehlgeschlagen) = zugangsdatenService.Versenden();

        if (gesendet + fehlgeschlagen == 0)
        {
            TempData["Fehler"] = "Alle G3-SuS haben schon Zugangsdaten. Es wurde nichts verschickt.";
        }
        else if (fehlgeschlagen > 0)
        {
            TempData["Fehler"] = $"{fehlgeschlagen} E-Mails konnten nicht verschickt werden. {gesendet} wurden verschickt.";
        }
        else
        {
            TempData["Erfolg"] = $"{gesendet} Zugangsdaten wurden verschickt.";
        }

        return RedirectToAction(nameof(Index));
    }

    private IActionResult Importiere(IFormFile? datei, Func<Stream, ImportErgebnisViewModel> laden)
    {
        var problem = datei is null || datei.Length == 0
            ? "Bitte wähle eine CSV-Datei aus."
            : !datei.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
                ? "Die Datei muss die Endung .csv haben."
                : datei.Length > MaxDateigroesse
                    ? "Die Datei ist zu gross (höchstens 1 MB)."
                    : null;

        if (problem is not null)
        {
            TempData["Fehler"] = problem;
            return RedirectToAction(nameof(Index));
        }

        using var strom = datei!.OpenReadStream();
        return View("Import", laden(strom));
    }
}
