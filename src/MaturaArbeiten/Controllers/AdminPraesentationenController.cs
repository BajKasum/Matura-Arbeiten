using MaturaArbeiten.Domain;
using MaturaArbeiten.Services;
using MaturaArbeiten.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaturaArbeiten.Controllers;

[Authorize(Roles = Rollen.Administration)]
public class AdminPraesentationenController(
    IPraesentationService service,
    IPraesentationVerwaltungService verwaltung) : Controller
{
    private const string BestaetigungenHinweis = "Schon verschickte Bestätigungen stimmen nicht mehr und sollten neu verschickt werden.";

    public IActionResult Index() => View(service.Alle());

    [HttpGet]
    public IActionResult Neu() => View("Formular", new PraesentationFormular());

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Neu(PraesentationFormular eingabe) =>
        Speichern(eingabe, verwaltung.Anlegen(eingabe), _ => "Die Präsentation wurde angelegt.");

    [HttpGet]
    public IActionResult Bearbeiten(int id) =>
        verwaltung.Finden(id) is { } formular ? View("Formular", formular) : NotFound();

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Bearbeiten(int id, PraesentationFormular eingabe) =>
        Speichern(eingabe, verwaltung.Aendern(id, eingabe), Geaendert);

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Loeschen(int id)
    {
        switch (verwaltung.Loeschen(id))
        {
            case PraesentationErgebnis.Erfolg erfolg:
                TempData["Erfolg"] = Geloescht(erfolg.BetroffeneEintragungen);
                break;
            case PraesentationErgebnis.Abgewiesen abgewiesen:
                TempData["Fehler"] = abgewiesen.Grund;
                break;
        }

        return RedirectToAction(nameof(Index));
    }

    private IActionResult Speichern(
        PraesentationFormular eingabe,
        PraesentationErgebnis ergebnis,
        Func<int, string> erfolgsmeldung)
    {
        if (ergebnis is PraesentationErgebnis.Abgewiesen abgewiesen)
        {
            ModelState.AddModelError(string.Empty, abgewiesen.Grund);
            return View("Formular", eingabe);
        }

        TempData["Erfolg"] = erfolgsmeldung(((PraesentationErgebnis.Erfolg)ergebnis).BetroffeneEintragungen);
        return RedirectToAction(nameof(Index));
    }

    private static string Geaendert(int eintragungen) =>
        eintragungen == 0
            ? "Die Änderung wurde gespeichert."
            : $"Die Änderung wurde gespeichert. Für diese Präsentation gibt es {Mehrzahl.Von(eintragungen, "Eintragung", "Eintragungen")}. {BestaetigungenHinweis}";

    private static string Geloescht(int eintragungen) =>
        eintragungen == 0
            ? "Die Präsentation wurde gelöscht."
            : $"Die Präsentation wurde gelöscht. {Mehrzahl.Von(eintragungen, "Eintragung wurde", "Eintragungen wurden")} mitgelöscht, die betroffenen G3-SuS können wieder wählen. {BestaetigungenHinweis}";
}
