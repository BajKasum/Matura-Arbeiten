using MaturaArbeiten.Data;
using MaturaArbeiten.Domain;
using MaturaArbeiten.ViewModels;

namespace MaturaArbeiten.Services;

public sealed class AdministrationService(
    AppDbContext db,
    IPraesentationService praesentationService,
    ILogger<AdministrationService> logger) : IAdministrationService
{
    public UebersichtViewModel Uebersicht()
    {
        var einstellung = db.Systemeinstellungen.Single();

        var mitPflicht = db.Eintragungen
            .GroupBy(e => e.G3susId)
            .Count(gruppe => gruppe.Count() >= Regeln.PflichtEintragungen);

        var protokoll = db.Protokoll();

        return new UebersichtViewModel(
            einstellung.EintragungsphaseOffen,
            db.Praesentationen.Count(),
            db.G3SuSListe.Count(),
            mitPflicht,
            Regeln.PflichtEintragungen,
            VersendetAn(protokoll, EmailProtokoll.Zugangsdaten),
            VersendetAn(protokoll, EmailProtokoll.Bestaetigung),
            EmailProtokoll.OffeneFehlschlaege(protokoll).Count,
            db.G3SuSListe.Count(g => g.PasswortHash == null),
            praesentationService.OhneMoeglichkeit());
    }

    public bool PhaseUmschalten()
    {
        var einstellung = db.Systemeinstellungen.Single();
        einstellung.EintragungsphaseOffen = !einstellung.EintragungsphaseOffen;
        db.SaveChanges();

        logger.LogInformation("Die Eintragungsphase wurde {Zustand}.", einstellung.EintragungsphaseOffen ? "geöffnet" : "geschlossen");

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

        logger.LogWarning("Das Schuljahr wurde zurückgesetzt.");
    }

    private static int VersendetAn(IEnumerable<ProtokollEintrag> protokoll, string typ) =>
        protokoll
            .Where(e => e.Typ == typ && e.Status == EmailProtokoll.Gesendet)
            .Select(e => e.G3susId)
            .Distinct()
            .Count();
}
