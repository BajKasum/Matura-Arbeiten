using System.Globalization;

namespace MaturaArbeiten.Domain;

public static class PraesentationPruefung
{
    private static readonly CultureInfo Kultur = CultureInfo.InvariantCulture;
    private static readonly string[] DatumFormate = ["dd.MM.yyyy", "d.M.yyyy"];
    private static readonly string[] UhrzeitFormate = ["HH:mm"];

    public static (PraesentationNeu? Wert, string? Grund) Pruefen(
        string? thema, string? datum, string? uhrzeit, string? raum) =>
        PruefeGetrimmt(
            thema?.Trim() ?? string.Empty,
            datum?.Trim() ?? string.Empty,
            uhrzeit?.Trim() ?? string.Empty,
            raum?.Trim() ?? string.Empty);

    public static (string Thema, DateOnly Datum, TimeOnly Uhrzeit, string Raum) Schluessel(PraesentationNeu p) =>
        (p.Thema.Trim().ToUpperInvariant(), p.Datum, p.Uhrzeit, p.Raum.Trim().ToUpperInvariant());

    public static bool Doppelt(PraesentationNeu neu, IEnumerable<PraesentationNeu> andere) =>
        andere.Select(Schluessel).Contains(Schluessel(neu));

    private static (PraesentationNeu? Wert, string? Grund) PruefeGetrimmt(
        string thema, string datum, string uhrzeit, string raum) =>
        thema.Length is 0 or > 200 ? (null, "Thema fehlt oder ist länger als 200 Zeichen.")
        : raum.Length is 0 or > 20 ? (null, "Raum fehlt oder ist länger als 20 Zeichen.")
        : !DateOnly.TryParseExact(datum, DatumFormate, Kultur, DateTimeStyles.None, out var d) ? (null, "Datum ist nicht im Format TT.MM.JJJJ.")
        : !TimeOnly.TryParseExact(uhrzeit, UhrzeitFormate, Kultur, DateTimeStyles.None, out var t) ? (null, "Uhrzeit ist nicht im Format HH:MM.")
        : (new PraesentationNeu(thema, d, t, raum), null);
}
