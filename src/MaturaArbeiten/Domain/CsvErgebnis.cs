namespace MaturaArbeiten.Domain;

public sealed record CsvErgebnis<T>(IReadOnlyList<T> Gueltig, IReadOnlyList<CsvFehler> Fehler);