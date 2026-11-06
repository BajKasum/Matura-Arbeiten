using MaturaArbeiten.Data;
using MaturaArbeiten.Domain;
using MaturaArbeiten.Models;
using MaturaArbeiten.ViewModels;

namespace MaturaArbeiten.Services;

public sealed class ImportService(AppDbContext db) : IImportService
{
    public ImportErgebnisViewModel PraesentationenLaden(Stream datei)
    {
        var ergebnis = CsvImport.PraesentationenPruefen(CsvLeser.Lesen(datei));

        db.Praesentationen.AddRange(ergebnis.Gueltig.Select(neu => new Praesentation
        {
            Thema = neu.Thema,
            Datum = neu.Datum,
            Uhrzeit = neu.Uhrzeit,
            Raum = neu.Raum
        }));
        db.SaveChanges();

        return new ImportErgebnisViewModel("Präsentationen", ergebnis.Gueltig.Count, ergebnis.Fehler);
    }

    public ImportErgebnisViewModel G3SuSLaden(Stream datei)
    {
        var vorhanden = db.G3SuSListe.Select(g => g.Email)
            .Concat(db.Administrationslogins.Select(a => a.Email))
            .ToList();

        var ergebnis = CsvImport.G3SuSPruefen(CsvLeser.Lesen(datei), vorhanden);

        db.G3SuSListe.AddRange(ergebnis.Gueltig.Select(neu => new G3SuS
        {
            Vorname = neu.Vorname,
            Nachname = neu.Nachname,
            Email = neu.Email
        }));
        db.SaveChanges();

        return new ImportErgebnisViewModel("G3-SuS", ergebnis.Gueltig.Count, ergebnis.Fehler);
    }
}