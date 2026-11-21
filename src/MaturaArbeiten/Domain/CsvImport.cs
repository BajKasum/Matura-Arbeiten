using System.Collections.Immutable;
using System.Net.Mail;

namespace MaturaArbeiten.Domain;

public static class CsvImport
{
    private static readonly string[] PraesentationKopf = ["Thema", "Datum", "Uhrzeit", "Raum"];
    private static readonly string[] G3SuSKopf = ["Vorname", "Nachname", "E-Mail"];

    private const string KeineDatenzeilen = "Die Datei enthält keine Datenzeilen.";

    public static CsvErgebnis<PraesentationNeu> PraesentationenPruefen(
        IEnumerable<CsvZeile> zeilen,
        IEnumerable<PraesentationNeu> vorhandene)
    {
        var datei = zeilen.ToList();

        if (Ablehnung(datei, PraesentationKopf) is { } grund)
        {
            return new CsvErgebnis<PraesentationNeu>([], [], grund);
        }

        var start = new PraesentationStand(
            ImmutableHashSet.CreateRange(vorhandene.Select(PraesentationPruefung.Schluessel)),
            ImmutableList<PraesentationNeu>.Empty,
            ImmutableList<CsvFehler>.Empty);

        var ende = datei
            .Skip(1)
            .Aggregate(start, VerarbeitePraesentation);

        return new CsvErgebnis<PraesentationNeu>(ende.Gueltig, ende.Fehler);
    }

    // Prüft alle Zeilen und sammelt die Fehler, statt beim ersten abzubrechen. Doppelt gilt auch gegen die schon gespeicherten E-Mail-Adressen.
    public static CsvErgebnis<G3SuSNeu> G3SuSPruefen(
        IEnumerable<CsvZeile> zeilen,
        IEnumerable<string> vorhandeneEmails)
    {
        var datei = zeilen.ToList();

        if (Ablehnung(datei, G3SuSKopf) is { } grund)
        {
            return new CsvErgebnis<G3SuSNeu>([], [], grund);
        }

        var start = new Stand(
            ImmutableHashSet.CreateRange(StringComparer.OrdinalIgnoreCase, vorhandeneEmails),
            ImmutableList<G3SuSNeu>.Empty,
            ImmutableList<CsvFehler>.Empty);

        var ende = datei
            .Skip(1)
            .Aggregate(start, Verarbeite);

        return new CsvErgebnis<G3SuSNeu>(ende.Gueltig, ende.Fehler);
    }

    private static string? Ablehnung(IReadOnlyList<CsvZeile> datei, string[] kopf) =>
        datei.Count == 0 ? KeineDatenzeilen
        : !IstKopfzeile(datei[0], kopf)
            ? $"Die Datei wurde nicht geladen. Die erste Zeile muss die Spaltennamen enthalten: {string.Join("; ", kopf)}."
        : datei.Count == 1 ? KeineDatenzeilen
        : null;

    private static bool IstKopfzeile(CsvZeile zeile, string[] kopf) =>
        zeile.Felder.Length == kopf.Length
        && zeile.Felder
            .Zip(kopf, (feld, name) => string.Equals(feld.Trim(), name, StringComparison.OrdinalIgnoreCase))
            .All(gleich => gleich);

    private sealed record Stand(
        ImmutableHashSet<string> Emails,
        ImmutableList<G3SuSNeu> Gueltig,
        ImmutableList<CsvFehler> Fehler);

    // Gibt den neuen Stand zurück, ohne den alten zu ändern: die Zeile ist ungültig, doppelt oder gültig.
    private static Stand Verarbeite(Stand stand, CsvZeile zeile) =>
        ZeileG3SuS(zeile.Felder) switch
        {
            (null, var grund) =>
                stand with { Fehler = stand.Fehler.Add(new CsvFehler(zeile.Nummer, grund!)) },
            ({ } wert, _) when stand.Emails.Contains(wert.Email) =>
                stand with { Fehler = stand.Fehler.Add(new CsvFehler(zeile.Nummer, "Diese E-Mail-Adresse ist doppelt.")) },
            ({ } wert, _) =>
                stand with { Emails = stand.Emails.Add(wert.Email), Gueltig = stand.Gueltig.Add(wert) }
        };

    private sealed record PraesentationStand(
        ImmutableHashSet<(string Thema, DateOnly Datum, TimeOnly Uhrzeit, string Raum)> Schluessel,
        ImmutableList<PraesentationNeu> Gueltig,
        ImmutableList<CsvFehler> Fehler);

    private static PraesentationStand VerarbeitePraesentation(PraesentationStand stand, CsvZeile zeile) =>
        ZeilePraesentation(zeile.Felder) switch
        {
            (null, var grund) =>
                stand with { Fehler = stand.Fehler.Add(new CsvFehler(zeile.Nummer, grund!)) },
            ({ } wert, _) when stand.Schluessel.Contains(PraesentationPruefung.Schluessel(wert)) =>
                stand with { Fehler = stand.Fehler.Add(new CsvFehler(zeile.Nummer, "Diese Präsentation ist doppelt.")) },
            ({ } wert, _) =>
                stand with { Schluessel = stand.Schluessel.Add(PraesentationPruefung.Schluessel(wert)), Gueltig = stand.Gueltig.Add(wert) }
        };

    private static (PraesentationNeu? Wert, string? Grund) ZeilePraesentation(string[] felder) =>
        felder is [var thema, var datum, var uhrzeit, var raum]
            ? PraesentationPruefung.Pruefen(thema, datum, uhrzeit, raum)
            : (null, "Die Zeile hat nicht genau vier Spalten.");

    private static (G3SuSNeu? Wert, string? Grund) ZeileG3SuS(string[] felder) =>
        felder is [var vorname, var nachname, var email]
            ? ZeileG3SuS(vorname.Trim(), nachname.Trim(), email.Trim())
            : (null, "Die Zeile hat nicht genau drei Spalten.");

    private static (G3SuSNeu? Wert, string? Grund) ZeileG3SuS(
        string vorname, string nachname, string email) =>
        vorname.Length is 0 or > 50 ? (null, "Vorname fehlt oder ist länger als 50 Zeichen.")
        : nachname.Length is 0 or > 50 ? (null, "Nachname fehlt oder ist länger als 50 Zeichen.")
        : email.Length is 0 or > 100 ? (null, "E-Mail-Adresse fehlt oder ist länger als 100 Zeichen.")
        : !GueltigeAdresse(email) ? (null, "Die E-Mail-Adresse ist nicht gültig.")
        : (new G3SuSNeu(vorname, nachname, email), null);

    private static bool GueltigeAdresse(string email) =>
        MailAddress.TryCreate(email, out var adresse) && adresse.Address == email;
}