using MaturaArbeiten.Data;
using MaturaArbeiten.Domain;
using MaturaArbeiten.Models;
using MaturaArbeiten.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MaturaArbeiten.Services;

public sealed class PraesentationService(AppDbContext db) : IPraesentationService
{
    public PraesentationenViewModel Liste(int g3susId)
    {
        var einstellung = db.Systemeinstellungen.Single();

        var praesentationen = db.Praesentationen
            .Include(p => p.Eintragungen)
            .OrderBy(p => p.Datum)
            .ThenBy(p => p.Uhrzeit)
            .ToList();

        var eigene = praesentationen
            .Where(p => p.Eintragungen.Any(e => e.G3susId == g3susId))
            .ToList();
        var eigeneIds = eigene.Select(p => p.Id).ToHashSet();
        var eigeneTermine = eigene.Select(p => p.Termin).ToList();

        var zeilen = praesentationen
            .Where(p => eigeneIds.Contains(p.Id) || IstWaehlbar(einstellung, p, eigeneTermine))
            .Select(p => new PraesentationZeile(
                p.Id,
                p.Thema,
                p.Datum,
                p.Uhrzeit,
                p.Raum,
                p.GetFreiePlaetze(einstellung.Platzkontingent),
                eigeneIds.Contains(p.Id)))
            .ToList();

        return new PraesentationenViewModel(
            zeilen,
            eigene.Count,
            Regeln.PflichtEintragungen,
            einstellung.EintragungsphaseOffen,
            NochErreichbar(einstellung, praesentationen, g3susId));
    }

    public AdminPraesentationenViewModel Alle()
    {
        var einstellung = db.Systemeinstellungen.Single();

        var zeilen = db.Praesentationen
            .Include(p => p.Eintragungen)
            .OrderBy(p => p.Datum)
            .ThenBy(p => p.Uhrzeit)
            .ThenBy(p => p.Raum)
            .ToList()
            .Select(p => new AdminPraesentationZeile(
                p.Id,
                p.Thema,
                p.Datum,
                p.Uhrzeit,
                p.Raum,
                p.Eintragungen.Count,
                p.GetFreiePlaetze(einstellung.Platzkontingent)))
            .ToList();

        return new AdminPraesentationenViewModel(zeilen, einstellung.Platzkontingent);
    }

    public IReadOnlyList<string> OhneMoeglichkeit()
    {
        var einstellung = db.Systemeinstellungen.Single();

        var praesentationen = db.Praesentationen
            .Include(p => p.Eintragungen)
            .ToList();

        if (praesentationen.Count == 0)
        {
            return [];
        }

        return db.G3SuSListe
            .OrderBy(g => g.Nachname)
            .ThenBy(g => g.Vorname)
            .ToList()
            .Where(g => !NochErreichbar(einstellung, praesentationen, g.Id))
            .Select(g => $"{g.Nachname} {g.Vorname}")
            .ToList();
    }

    private static bool IstWaehlbar(
        Systemeinstellung einstellung,
        Praesentation praesentation,
        IReadOnlyCollection<Termin> eigeneTermine) =>
        Regeln.Pruefe(
            einstellung.EintragungsphaseOffen,
            einstellung.Platzkontingent,
            praesentation.Termin,
            praesentation.Eintragungen.Count,
            eigeneTermine) is EintragungErgebnis.Erfolg;

    private static bool NochErreichbar(
        Systemeinstellung einstellung,
        IReadOnlyList<Praesentation> praesentationen,
        int g3susId)
    {
        var eigene = praesentationen
            .Where(p => p.Eintragungen.Any(e => e.G3susId == g3susId))
            .ToList();
        var eigeneIds = eigene.Select(p => p.Id).ToHashSet();
        var eigeneTermine = eigene.Select(p => p.Termin).ToList();

        var waehlbareTermine = praesentationen
            .Where(p => !eigeneIds.Contains(p.Id))
            .Where(p => Regeln.Pruefe(
                true,
                einstellung.Platzkontingent,
                p.Termin,
                p.Eintragungen.Count,
                eigeneTermine) is EintragungErgebnis.Erfolg)
            .Select(p => p.Termin);

        return Regeln.NochErreichbar(eigeneTermine, waehlbareTermine);
    }
}
