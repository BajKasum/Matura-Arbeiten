using MaturaArbeiten.Data;
using MaturaArbeiten.Domain;
using MaturaArbeiten.Models;
using MaturaArbeiten.ViewModels;

namespace MaturaArbeiten.Services;

public sealed class ImportService(AppDbContext db) : IImportService
{
    public ImportErgebnisViewModel PraesentationenLaden(Stream datei)
    {
        var lesung = CsvLeser.Lesen(datei);

        if (lesung.Ablehnung is not null)
        {
            return Abgelehnt("Präsentationen", lesung.Ablehnung);
        }

        var vorhandene = db.Praesentationen
            .Select(p => new PraesentationNeu(p.Thema, p.Datum, p.Uhrzeit, p.Raum))
            .ToList();

        var ergebnis = CsvImport.PraesentationenPruefen(lesung.Zeilen, vorhandene);

        db.Praesentationen.AddRange(ergebnis.Gueltig.Select(neu => new Praesentation
        {
            Thema = neu.Thema,
            Datum = neu.Datum,
            Uhrzeit = neu.Uhrzeit,
            Raum = neu.Raum
        }));
        db.SaveChanges();

        return new ImportErgebnisViewModel("Präsentationen", ergebnis.Gueltig.Count, ergebnis.Fehler, ergebnis.Ablehnung);
    }

    public ImportErgebnisViewModel G3SuSLaden(Stream datei)
    {
        var lesung = CsvLeser.Lesen(datei);

        if (lesung.Ablehnung is not null)
        {
            return Abgelehnt("G3-SuS", lesung.Ablehnung);
        }

        var vorhanden = db.G3SuSListe.Select(g => g.Email)
            .Concat(db.Administrationslogins.Select(a => a.Email))
            .ToList();

        var ergebnis = CsvImport.G3SuSPruefen(lesung.Zeilen, vorhanden);

        db.G3SuSListe.AddRange(ergebnis.Gueltig.Select(neu => new G3SuS
        {
            Vorname = neu.Vorname,
            Nachname = neu.Nachname,
            Email = neu.Email
        }));
        db.SaveChanges();

        return new ImportErgebnisViewModel("G3-SuS", ergebnis.Gueltig.Count, ergebnis.Fehler, ergebnis.Ablehnung);
    }

    private static ImportErgebnisViewModel Abgelehnt(string titel, string grund) =>
        new(titel, 0, [], grund);
}