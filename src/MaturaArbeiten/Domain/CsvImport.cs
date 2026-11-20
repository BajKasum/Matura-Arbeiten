using System.Collections.Immutable;
using System.Globalization;
using System.Net.Mail;

namespace MaturaArbeiten.Domain;

public static class CsvImport
{
    private static readonly CultureInfo Kultur = CultureInfo.InvariantCulture;
    private static readonly string[] DatumFormate = ["dd.MM.yyyy", "d.M.yyyy"];
    private static readonly string[] UhrzeitFormate = ["HH:mm", "H:mm", "HH:mm:ss"];

    public static CsvErgebnis<PraesentationNeu> PraesentationenPruefen(
        IEnumerable<CsvZeile> zeilen,
        IEnumerable<PraesentationNeu> vorhandene)
    {
        var start = new PraesentationStand(
            ImmutableHashSet.CreateRange(vorhandene.Select(Schluessel)),
            ImmutableList<PraesentationNeu>.Empty,
            ImmutableList<CsvFehler>.Empty);

        var ende = zeilen
            .Skip(1)
            .Aggregate(start, VerarbeitePraesentation);

        return new CsvErgebnis<PraesentationNeu>(ende.Gueltig, ende.Fehler);
    }

    public static CsvErgebnis<G3SuSNeu> G3SuSPruefen(
        IEnumerable<CsvZeile> zeilen,
        IEnumerable<string> vorhandeneEmails)
    {
        var start = new Stand(
            ImmutableHashSet.CreateRange(StringComparer.OrdinalIgnoreCase, vorhandeneEmails),
            ImmutableList<G3SuSNeu>.Empty,
            ImmutableList<CsvFehler>.Empty);

        var ende = zeilen
            .Skip(1)
            .Aggregate(start, Verarbeite);

        return new CsvErgebnis<G3SuSNeu>(ende.Gueltig, ende.Fehler);
    }

    private sealed record Stand(
        ImmutableHashSet<string> Emails,
        ImmutableList<G3SuSNeu> Gueltig,
        ImmutableList<CsvFehler> Fehler);

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
            ({ } wert, _) when stand.Schluessel.Contains(Schluessel(wert)) =>
                stand with { Fehler = stand.Fehler.Add(new CsvFehler(zeile.Nummer, "Diese Präsentation ist doppelt.")) },
            ({ } wert, _) =>
                stand with { Schluessel = stand.Schluessel.Add(Schluessel(wert)), Gueltig = stand.Gueltig.Add(wert) }
        };

    private static (string Thema, DateOnly Datum, TimeOnly Uhrzeit, string Raum) Schluessel(PraesentationNeu p) =>
        (p.Thema.Trim().ToUpperInvariant(), p.Datum, p.Uhrzeit, p.Raum.Trim().ToUpperInvariant());

    private static (PraesentationNeu? Wert, string? Grund) ZeilePraesentation(string[] felder) =>
        felder is [var thema, var datum, var uhrzeit, var raum]
            ? ZeilePraesentation(thema.Trim(), datum.Trim(), uhrzeit.Trim(), raum.Trim())
            : (null, "Die Zeile hat nicht genau vier Spalten.");

    private static (PraesentationNeu? Wert, string? Grund) ZeilePraesentation(
        string thema, string datum, string uhrzeit, string raum) =>
        thema.Length is 0 or > 200 ? (null, "Thema fehlt oder ist länger als 200 Zeichen.")
        : raum.Length is 0 or > 20 ? (null, "Raum fehlt oder ist länger als 20 Zeichen.")
        : !DateOnly.TryParseExact(datum, DatumFormate, Kultur, DateTimeStyles.None, out var d) ? (null, "Datum ist nicht im Format TT.MM.JJJJ.")
        : !TimeOnly.TryParseExact(uhrzeit, UhrzeitFormate, Kultur, DateTimeStyles.None, out var t) ? (null, "Uhrzeit ist nicht im Format HH:MM.")
        : (new PraesentationNeu(thema, d, t, raum), null);

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