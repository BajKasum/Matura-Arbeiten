namespace MaturaArbeiten.Domain;

public sealed record CsvLesung(IReadOnlyList<CsvZeile> Zeilen, string? Ablehnung);
