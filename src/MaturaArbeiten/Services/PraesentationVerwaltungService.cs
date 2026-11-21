using System.Globalization;
using MaturaArbeiten.Data;
using MaturaArbeiten.Domain;
using MaturaArbeiten.Models;
using MaturaArbeiten.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MaturaArbeiten.Services;

public sealed class PraesentationVerwaltungService(AppDbContext db) : IPraesentationVerwaltungService
{
    private const string Unbekannt = "Diese Präsentation gibt es nicht mehr.";
    private const string Doppelt = "Diese Präsentation gibt es schon.";
    private const string Konflikt = "Gerade wird gleichzeitig gearbeitet. Die Änderung wurde nicht gespeichert, bitte versuche es noch einmal.";

    private static readonly CultureInfo Kultur = CultureInfo.InvariantCulture;

    public PraesentationFormular? Finden(int id) =>
        db.Praesentationen
            .Where(p => p.Id == id)
            .ToList()
            .Select(p => new PraesentationFormular
            {
                Thema = p.Thema,
                Datum = p.Datum.ToString("dd.MM.yyyy", Kultur),
                Uhrzeit = p.Uhrzeit.ToString("HH:mm", Kultur),
                Raum = p.Raum
            })
            .SingleOrDefault();

    public PraesentationErgebnis Anlegen(PraesentationFormular eingabe) =>
        PraesentationPruefung.Pruefen(eingabe.Thema, eingabe.Datum, eingabe.Uhrzeit, eingabe.Raum) switch
        {
            (null, var grund) => new PraesentationErgebnis.Abgewiesen(grund!),
            ({ } neu, _) when GibtEsSchon(neu, null) => new PraesentationErgebnis.Abgewiesen(Doppelt),
            ({ } neu, _) => Speichern(neu)
        };

    public PraesentationErgebnis Aendern(int id, PraesentationFormular eingabe) =>
        PraesentationPruefung.Pruefen(eingabe.Thema, eingabe.Datum, eingabe.Uhrzeit, eingabe.Raum) switch
        {
            (null, var grund) => new PraesentationErgebnis.Abgewiesen(grund!),
            ({ } neu, _) => db.Versuchen(() => AendernEinmal(id, neu))
                ?? new PraesentationErgebnis.Abgewiesen(Konflikt)
        };

    public PraesentationErgebnis Loeschen(int id) =>
        db.Versuchen(() => LoeschenEinmal(id))
        ?? new PraesentationErgebnis.Abgewiesen(Konflikt);

    private PraesentationErgebnis Speichern(PraesentationNeu neu)
    {
        db.Praesentationen.Add(new Praesentation
        {
            Thema = neu.Thema,
            Datum = neu.Datum,
            Uhrzeit = neu.Uhrzeit,
            Raum = neu.Raum
        });
        db.SaveChanges();

        return new PraesentationErgebnis.Erfolg(0);
    }

    private PraesentationErgebnis AendernEinmal(int id, PraesentationNeu neu)
    {
        var praesentation = db.Praesentationen
            .Include(p => p.Eintragungen)
            .SingleOrDefault(p => p.Id == id);

        if (praesentation is null)
        {
            return new PraesentationErgebnis.Abgewiesen(Unbekannt);
        }

        if (GibtEsSchon(neu, id))
        {
            return new PraesentationErgebnis.Abgewiesen(Doppelt);
        }

        var betroffene = Kollisionen(praesentation, new Termin(neu.Datum, neu.Uhrzeit));

        if (betroffene.Count > 0)
        {
            return new PraesentationErgebnis.Abgewiesen(
                "Die Änderung wurde nicht gespeichert. Diese G3-SuS hätten dann zwei Präsentationen zur gleichen Zeit: "
                + string.Join(", ", betroffene)
                + ". Trage sie zuerst unter Eintragungen verwalten um.");
        }

        var geaendert = praesentation.Thema != neu.Thema
            || praesentation.Datum != neu.Datum
            || praesentation.Uhrzeit != neu.Uhrzeit
            || praesentation.Raum != neu.Raum;

        praesentation.Thema = neu.Thema;
        praesentation.Datum = neu.Datum;
        praesentation.Uhrzeit = neu.Uhrzeit;
        praesentation.Raum = neu.Raum;
        db.SaveChanges();

        return new PraesentationErgebnis.Erfolg(geaendert ? praesentation.Eintragungen.Count : 0);
    }

    private PraesentationErgebnis LoeschenEinmal(int id)
    {
        var praesentation = db.Praesentationen.SingleOrDefault(p => p.Id == id);

        if (praesentation is null)
        {
            return new PraesentationErgebnis.Abgewiesen(Unbekannt);
        }

        var eintragungen = db.Eintragungen.Count(e => e.PraesentationId == id);

        db.Praesentationen.Remove(praesentation);
        db.SaveChanges();

        return new PraesentationErgebnis.Erfolg(eintragungen);
    }

    private bool GibtEsSchon(PraesentationNeu neu, int? ausserId) =>
        PraesentationPruefung.Doppelt(
            neu,
            db.Praesentationen
                .Where(p => p.Id != ausserId)
                .Select(p => new PraesentationNeu(p.Thema, p.Datum, p.Uhrzeit, p.Raum))
                .ToList());

    private IReadOnlyList<string> Kollisionen(Praesentation praesentation, Termin neuerTermin)
    {
        if (praesentation.Termin == neuerTermin)
        {
            return [];
        }

        var eingetragene = praesentation.Eintragungen.Select(e => e.G3susId).ToList();

        var betroffeneIds = db.Eintragungen
            .Where(e => eingetragene.Contains(e.G3susId) && e.PraesentationId != praesentation.Id)
            .Join(db.Praesentationen, e => e.PraesentationId, p => p.Id, (e, p) => new { e.G3susId, p.Datum, p.Uhrzeit })
            .ToList()
            .Where(x => Regeln.Kollidiert(new Termin(x.Datum, x.Uhrzeit), neuerTermin))
            .Select(x => x.G3susId)
            .Distinct()
            .ToList();

        return db.G3SuSListe
            .Where(g => betroffeneIds.Contains(g.Id))
            .OrderBy(g => g.Nachname)
            .ThenBy(g => g.Vorname)
            .ToList()
            .Select(g => $"{g.Nachname} {g.Vorname}")
            .ToList();
    }
}
