using System.Data;
using MaturaArbeiten.Data;
using MaturaArbeiten.Domain;
using MaturaArbeiten.Models;
using MaturaArbeiten.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MaturaArbeiten.Services;

public sealed class EintragungService(AppDbContext db, TimeProvider zeit) : IEintragungService
{
    public EintragungErgebnis Eintragen(int g3susId, int praesentationId)
    {
        using var transaktion = db.Database.BeginTransaction(IsolationLevel.Serializable);

        var ergebnis = PruefeRegeln(g3susId, praesentationId);

        if (ergebnis is EintragungErgebnis.Erfolg)
        {
            db.Eintragungen.Add(new Eintragung
            {
                G3susId = g3susId,
                PraesentationId = praesentationId,
                ErstelltAm = zeit.GetUtcNow().UtcDateTime
            });
            db.SaveChanges();
            transaktion.Commit();
        }

        return ergebnis;
    }

    public MeineEintragungenViewModel Meine(int g3susId)
    {
        var zeilen = db.Eintragungen
            .Where(e => e.G3susId == g3susId)
            .Join(
                db.Praesentationen,
                e => e.PraesentationId,
                p => p.Id,
                (e, p) => new { p.Thema, p.Datum, p.Uhrzeit, p.Raum })
            .OrderBy(x => x.Datum)
            .ThenBy(x => x.Uhrzeit)
            .ToList()
            .Select(x => new EintragungZeile(x.Thema, x.Datum, x.Uhrzeit, x.Raum))
            .ToList();

        return new MeineEintragungenViewModel(zeilen, Regeln.PflichtEintragungen);
    }

    private EintragungErgebnis PruefeRegeln(int g3susId, int praesentationId)
    {
        var einstellung = db.Systemeinstellungen.Single();
        var wunsch = db.Praesentationen.Single(p => p.Id == praesentationId);
        var belegt = db.Eintragungen.Count(e => e.PraesentationId == praesentationId);

        var eigene = db.Eintragungen
            .Where(e => e.G3susId == g3susId)
            .Join(
                db.Praesentationen,
                e => e.PraesentationId,
                p => p.Id,
                (e, p) => new { p.Datum, p.Uhrzeit })
            .ToList()
            .Select(x => new Termin(x.Datum, x.Uhrzeit))
            .ToList();

        return Regeln.Pruefe(
            einstellung.EintragungsphaseOffen,
            einstellung.Platzkontingent,
            wunsch.Termin,
            belegt,
            eigene);
    }
}