using MaturaArbeiten.Domain;
using MaturaArbeiten.Services;
using MaturaArbeiten.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaturaArbeiten.Controllers;

[Authorize(Roles = Rollen.Administration)]
public class AdminController(
    IImportService importService,
    IZugangsdatenService zugangsdatenService) : Controller
{
    private const long MaxDateigroesse = 1_000_000;

    public IActionResult Index() => View();

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