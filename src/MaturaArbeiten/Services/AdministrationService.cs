using MaturaArbeiten.Data;
using MaturaArbeiten.Domain;
using MaturaArbeiten.ViewModels;

namespace MaturaArbeiten.Services;

public sealed class AdministrationService(AppDbContext db) : IAdministrationService
{
    public UebersichtViewModel Uebersicht()
    {
        var einstellung = db.Systemeinstellungen.Single();

        var mitPflicht = db.Eintragungen
            .GroupBy(e => e.G3susId)
            .Count(gruppe => gruppe.Count() >= Regeln.PflichtEintragungen);

        var zugangsdaten = db.EmailVersandListe
            .Where(m => m.Typ == "Zugangsdaten")
            .Select(m => new { m.G3susId, m.Status })
            .ToList();

        var versendet = zugangsdaten
            .Where(z => z.Status == "Gesendet")
            .Select(z => z.G3susId)
            .Distinct()
            .ToList();

        var fehlgeschlagen = zugangsdaten
            .Where(z => z.Status == "Fehlgeschlagen")
            .Select(z => z.G3susId)
            .Distinct()
            .Except(versendet)
            .Count();

        return new UebersichtViewModel(
            einstellung.EintragungsphaseOffen,
            db.G3SuSListe.Count(),
            mitPflicht,
            Regeln.PflichtEintragungen,
            versendet.Count,
            fehlgeschlagen);
    }

    public bool PhaseUmschalten()
    {
        var einstellung = db.Systemeinstellungen.Single();
        einstellung.EintragungsphaseOffen = !einstellung.EintragungsphaseOffen;
        db.SaveChanges();

        return einstellung.EintragungsphaseOffen;
    }

    public ZuruecksetzenViewModel Vorschau() =>
        new(
            db.G3SuSListe.Count(),
            db.Praesentationen.Count(),
            db.Eintragungen.Count(),
            db.EmailVersandListe.Count());

    public void Zuruecksetzen()
    {
        db.EmailVersandListe.RemoveRange(db.EmailVersandListe);
        db.Eintragungen.RemoveRange(db.Eintragungen);
        db.G3SuSListe.RemoveRange(db.G3SuSListe);
        db.Praesentationen.RemoveRange(db.Praesentationen);
        db.Systemeinstellungen.Single().EintragungsphaseOffen = true;

        db.SaveChanges();
    }
}
