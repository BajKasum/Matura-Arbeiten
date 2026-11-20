using MaturaArbeiten.Data;
using MaturaArbeiten.Domain;
using MaturaArbeiten.Models;

namespace MaturaArbeiten.Services;

public sealed class BestaetigungService(
    AppDbContext db,
    IEmailService emailService,
    TimeProvider zeit) : IBestaetigungService
{
    private sealed record Daten(G3SuS? Schueler, IReadOnlyList<MailEintragung> Eintragungen);

    public Versand Senden(int g3susId)
    {
        var daten = db.Versuchen(() => Laden(g3susId));

        if (daten is null)
        {
            return Versand.Fehlgeschlagen;
        }

        var (schueler, eintragungen) = daten;

        if (schueler is null || eintragungen.Count < Regeln.PflichtEintragungen)
        {
            return Versand.NichtNoetig;
        }

        var erfolg = emailService.Senden(Mailtexte.Bestaetigung(schueler.Vorname, schueler.Email, eintragungen));

        db.EmailVersandListe.Add(new EmailVersand
        {
            G3susId = g3susId,
            Typ = "Bestaetigung",
            Status = erfolg ? "Gesendet" : "Fehlgeschlagen",
            GesendetAm = zeit.GetUtcNow().UtcDateTime
        });
        db.SaveChanges();

        return erfolg ? Versand.Gesendet : Versand.Fehlgeschlagen;
    }

    private Daten Laden(int g3susId)
    {
        var schueler = db.G3SuSListe.SingleOrDefault(g => g.Id == g3susId);

        var eintragungen = db.Eintragungen
            .Where(e => e.G3susId == g3susId)
            .Join(
                db.Praesentationen,
                e => e.PraesentationId,
                p => p.Id,
                (e, p) => new { p.Thema, p.Datum, p.Uhrzeit, p.Raum })
            .OrderBy(x => x.Datum)
            .ThenBy(x => x.Uhrzeit)
            .ToList()
            .Select(x => new MailEintragung(x.Thema, x.Datum, x.Uhrzeit, x.Raum))
            .ToList();

        return new Daten(schueler, eintragungen);
    }

    public (int Gesendet, int Fehlgeschlagen) AnAlle()
    {
        var ergebnisse = db.Eintragungen
            .GroupBy(e => e.G3susId)
            .Where(gruppe => gruppe.Count() >= Regeln.PflichtEintragungen)
            .Select(gruppe => gruppe.Key)
            .ToList()
            .Select(Senden)
            .ToList();

        return (
            ergebnisse.Count(v => v == Versand.Gesendet),
            ergebnisse.Count(v => v == Versand.Fehlgeschlagen));
    }
}
