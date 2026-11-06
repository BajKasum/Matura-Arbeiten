using System.Data;
using MaturaArbeiten.Data;
using MaturaArbeiten.Domain;
using MaturaArbeiten.Models;
using MaturaArbeiten.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MaturaArbeiten.Services;

public sealed class EintragungService(AppDbContext db, TimeProvider zeit) : IEintragungService
{
    public EintragungErgebnis Eintragen(int g3susId, int praesentationId) =>
        Speichern(g3susId, praesentationId, false);

    public EintragungErgebnis EintragenDurchAdministration(int g3susId, int praesentationId) =>
        Speichern(g3susId, praesentationId, true);

    public EintragungErgebnis? Aendern(int id, int neuePraesentationId)
    {
        using var transaktion = db.Database.BeginTransaction(IsolationLevel.Serializable);

        var eintragung = db.Eintragungen.SingleOrDefault(e => e.Id == id);

        if (eintragung is null)
        {
            return null;
        }

        var ergebnis = PruefeRegeln(eintragung.G3susId, neuePraesentationId, true, id);

        if (ergebnis is EintragungErgebnis.Erfolg)
        {
            eintragung.PraesentationId = neuePraesentationId;
            db.SaveChanges();
            transaktion.Commit();
        }

        return ergebnis;
    }

    public bool Loeschen(int id)
    {
        var eintragung = db.Eintragungen.SingleOrDefault(e => e.Id == id);

        if (eintragung is null)
        {
            return false;
        }

        db.Eintragungen.Remove(eintragung);
        db.SaveChanges();

        return true;
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

    public AdminEintragungenViewModel Alle()
    {
        var zeilen = db.Eintragungen
            .Join(db.G3SuSListe, e => e.G3susId, g => g.Id, (e, g) => new { e, g })
            .Join(db.Praesentationen, x => x.e.PraesentationId, p => p.Id, (x, p) => new { x.e, x.g, p })
            .OrderBy(x => x.g.Nachname)
            .ThenBy(x => x.g.Vorname)
            .ThenBy(x => x.p.Datum)
            .ThenBy(x => x.p.Uhrzeit)
            .Select(x => new
            {
                x.e.Id,
                x.g.Vorname,
                x.g.Nachname,
                x.p.Thema,
                x.p.Datum,
                x.p.Uhrzeit,
                x.p.Raum,
                x.e.ErstelltAm
            })
            .ToList()
            .Select(x => new AdminEintragungZeile(
                x.Id,
                $"{x.Nachname} {x.Vorname}",
                x.Thema,
                x.Datum,
                x.Uhrzeit,
                x.Raum,
                x.ErstelltAm))
            .ToList();

        return new AdminEintragungenViewModel(zeilen);
    }

    public EintragungFormularViewModel? Formular(int? id)
    {
        var eingabe = new EintragungFormular();
        string? name = null;

        if (id is not null)
        {
            var eintragung = db.Eintragungen.SingleOrDefault(e => e.Id == id);

            if (eintragung is null)
            {
                return null;
            }

            eingabe = new EintragungFormular
            {
                G3susId = eintragung.G3susId,
                PraesentationId = eintragung.PraesentationId
            };
            name = db.G3SuSListe
                .Where(g => g.Id == eintragung.G3susId)
                .Select(g => g.Nachname + " " + g.Vorname)
                .Single();
        }

        var einstellung = db.Systemeinstellungen.Single();

        var anzahlJeG3SuS = db.Eintragungen
            .GroupBy(e => e.G3susId)
            .Select(gruppe => new { Id = gruppe.Key, Anzahl = gruppe.Count() })
            .ToDictionary(x => x.Id, x => x.Anzahl);

        var g3sus = db.G3SuSListe
            .OrderBy(g => g.Nachname)
            .ThenBy(g => g.Vorname)
            .ToList()
            .Select(g => new Auswahl(
                g.Id,
                $"{g.Nachname} {g.Vorname} ({anzahlJeG3SuS.GetValueOrDefault(g.Id)} von {Regeln.PflichtEintragungen})"))
            .ToList();

        var praesentationen = db.Praesentationen
            .Include(p => p.Eintragungen)
            .OrderBy(p => p.Datum)
            .ThenBy(p => p.Uhrzeit)
            .ToList()
            .Select(p => new Auswahl(
                p.Id,
                $"{p.Thema}, {p.Datum:dd.MM.yyyy} {p.Uhrzeit:HH:mm}, Raum {p.Raum} ({p.GetFreiePlaetze(einstellung.Platzkontingent)} frei)"))
            .ToList();

        return new EintragungFormularViewModel(eingabe, g3sus, praesentationen, name);
    }

    private EintragungErgebnis Speichern(int g3susId, int praesentationId, bool phaseOffenErzwingen)
    {
        using var transaktion = db.Database.BeginTransaction(IsolationLevel.Serializable);

        var ergebnis = PruefeRegeln(g3susId, praesentationId, phaseOffenErzwingen);

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

    private EintragungErgebnis PruefeRegeln(
        int g3susId,
        int praesentationId,
        bool phaseOffenErzwingen,
        int? ausserEintragungId = null)
    {
        var einstellung = db.Systemeinstellungen.Single();
        var wunsch = db.Praesentationen.Single(p => p.Id == praesentationId);
        var belegt = db.Eintragungen.Count(e => e.PraesentationId == praesentationId && e.Id != ausserEintragungId);

        var eigene = db.Eintragungen
            .Where(e => e.G3susId == g3susId && e.Id != ausserEintragungId)
            .Join(
                db.Praesentationen,
                e => e.PraesentationId,
                p => p.Id,
                (e, p) => new { p.Datum, p.Uhrzeit })
            .ToList()
            .Select(x => new Termin(x.Datum, x.Uhrzeit))
            .ToList();

        return Regeln.Pruefe(
            einstellung.EintragungsphaseOffen || phaseOffenErzwingen,
            einstellung.Platzkontingent,
            wunsch.Termin,
            belegt,
            eigene);
    }
}
