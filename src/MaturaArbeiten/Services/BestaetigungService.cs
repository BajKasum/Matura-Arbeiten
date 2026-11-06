using MaturaArbeiten.Data;
using MaturaArbeiten.Domain;
using MaturaArbeiten.Models;

namespace MaturaArbeiten.Services;

public sealed class BestaetigungService(
    AppDbContext db,
    IEmailService emailService,
    TimeProvider zeit) : IBestaetigungService
{
    public Versand Senden(int g3susId)
    {
        var schueler = db.G3SuSListe.SingleOrDefault(g => g.Id == g3susId);

        if (schueler is null)
        {
            return Versand.NichtNoetig;
        }

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

        if (eintragungen.Count < Regeln.PflichtEintragungen)
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
