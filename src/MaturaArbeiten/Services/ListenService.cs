using MaturaArbeiten.Data;
using MaturaArbeiten.Domain;
using MaturaArbeiten.Models;
using MaturaArbeiten.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MaturaArbeiten.Services;

public sealed class ListenService(AppDbContext db) : IListenService
{
    public RaumuebersichtViewModel Raumuebersicht()
    {
        var g3sus = db.G3SuSListe.ToDictionary(g => g.Id);

        var raeume = db.Praesentationen
            .Include(p => p.Eintragungen)
            .ToList()
            .GroupBy(p => p.Raum)
            .OrderBy(gruppe => gruppe.Key, StringComparer.Ordinal)
            .Select(gruppe => new RaumGruppe(
                gruppe.Key,
                gruppe
                    .OrderBy(p => p.Datum)
                    .ThenBy(p => p.Uhrzeit)
                    .Select(p => new RaumPraesentation(
                        p.Id,
                        p.Thema,
                        p.Datum,
                        p.Uhrzeit,
                        Zuseher(p, g3sus).Select(z => $"{z.Nachname} {z.Vorname}").ToList()))
                    .ToList()))
            .ToList();

        return new RaumuebersichtViewModel(raeume);
    }

    public string RaumuebersichtCsv()
    {
        var kopf = new[] { "Raum", "Datum", "Uhrzeit", "Thema", "Zuseher" };

        var zeilen = Raumuebersicht().Raeume
            .SelectMany(raum => raum.Praesentationen.Select(p => new[]
            {
                raum.Raum,
                p.Datum.ToString("dd.MM.yyyy"),
                p.Uhrzeit.ToString("HH:mm"),
                p.Thema,
                string.Join(", ", p.Zuseher)
            }));

        return CsvExport.Datei(new[] { kopf }.Concat(zeilen));
    }

    public ZuseherlisteViewModel? Zuseherliste(int? praesentationId)
    {
        var einstellung = db.Systemeinstellungen.Single();

        var praesentationen = db.Praesentationen
            .Include(p => p.Eintragungen)
            .OrderBy(p => p.Datum)
            .ThenBy(p => p.Uhrzeit)
            .ThenBy(p => p.Raum)
            .ToList();

        var auswahl = praesentationen
            .Select(p => new Auswahl(
                p.Id,
                $"{p.Thema}, {p.Datum:dd.MM.yyyy} {p.Uhrzeit:HH:mm}, Raum {p.Raum}"))
            .ToList();

        var gewaehlt = praesentationId is null
            ? praesentationen.FirstOrDefault()
            : praesentationen.SingleOrDefault(p => p.Id == praesentationId);

        if (praesentationId is not null && gewaehlt is null)
        {
            return null;
        }

        if (gewaehlt is null)
        {
            return new ZuseherlisteViewModel(auswahl, null, []);
        }

        var g3sus = db.G3SuSListe.ToDictionary(g => g.Id);

        return new ZuseherlisteViewModel(
            auswahl,
            new AdminPraesentationZeile(
                gewaehlt.Id,
                gewaehlt.Thema,
                gewaehlt.Datum,
                gewaehlt.Uhrzeit,
                gewaehlt.Raum,
                gewaehlt.Eintragungen.Count,
                gewaehlt.GetFreiePlaetze(einstellung.Platzkontingent)),
            Zuseher(gewaehlt, g3sus)
                .Select(z => new ZuseherZeile(z.Nachname, z.Vorname, z.Email))
                .ToList());
    }

    public string? ZuseherlisteCsv(int praesentationId)
    {
        var liste = Zuseherliste(praesentationId);

        if (liste?.Gewaehlt is not { } p)
        {
            return null;
        }

        var kopf = new[] { "Thema", "Datum", "Uhrzeit", "Raum", "Nachname", "Vorname", "E-Mail" };

        var zeilen = liste.Zuseher.Select(z => new[]
        {
            p.Thema,
            p.Datum.ToString("dd.MM.yyyy"),
            p.Uhrzeit.ToString("HH:mm"),
            p.Raum,
            z.Nachname,
            z.Vorname,
            z.Email
        });

        return CsvExport.Datei(new[] { kopf }.Concat(zeilen));
    }

    private static IEnumerable<G3SuS> Zuseher(Praesentation praesentation, IReadOnlyDictionary<int, G3SuS> g3sus) =>
        praesentation.Eintragungen
            .Select(e => g3sus[e.G3susId])
            .OrderBy(g => g.Nachname, StringComparer.CurrentCulture)
            .ThenBy(g => g.Vorname, StringComparer.CurrentCulture);
}
