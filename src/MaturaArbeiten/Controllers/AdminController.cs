using MaturaArbeiten.Domain;
using MaturaArbeiten.Services;
using MaturaArbeiten.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MaturaArbeiten.Controllers;

[Authorize(Roles = Rollen.Administration)]
public class AdminController(
    IAdministrationService administrationService,
    IImportService importService,
    IZugangsdatenService zugangsdatenService,
    IBestaetigungService bestaetigungService,
    IMemoryCache zwischenspeicher) : Controller
{
    private const long MaxDateigroesse = 1_000_000;
    private const string Bestaetigungswort = "ZURÜCKSETZEN";
    private static readonly TimeSpan ErgebnisDauer = TimeSpan.FromMinutes(30);

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
        if (!string.Equals(bestaetigung?.Trim(), Bestaetigungswort, StringComparison.OrdinalIgnoreCase))
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

    public IActionResult ImportErgebnis(Guid id)
    {
        if (zwischenspeicher.TryGetValue(id, out ImportErgebnisViewModel? ergebnis) && ergebnis is not null)
        {
            return View("Import", ergebnis);
        }

        TempData["Fehler"] = "Dieses Import-Ergebnis gibt es nicht mehr.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ZugangsdatenVersenden()
    {
        var (gesendet, fehlgeschlagen) = zugangsdatenService.Versenden();
        var (erfolg, text) = Meldungen.Versandergebnis(
            gesendet,
            fehlgeschlagen,
            EmailProtokoll.Zugangsdaten,
            "Alle G3-SuS haben schon Zugangsdaten. Es wurde nichts verschickt.");

        TempData[erfolg ? "Erfolg" : "Fehler"] = text;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult BestaetigungenVersenden()
    {
        var (gesendet, fehlgeschlagen) = bestaetigungService.AnAlle();
        var (erfolg, text) = Meldungen.Versandergebnis(
            gesendet,
            fehlgeschlagen,
            "Bestätigungen",
            "Kein G3-SuS hat schon zwei Eintragungen. Es wurde nichts verschickt.");

        TempData[erfolg ? "Erfolg" : "Fehler"] = text;

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
        var ergebnis = laden(strom);

        var id = Guid.NewGuid();
        zwischenspeicher.Set(id, ergebnis, ErgebnisDauer);

        return RedirectToAction(nameof(ImportErgebnis), new { id });
    }
}
