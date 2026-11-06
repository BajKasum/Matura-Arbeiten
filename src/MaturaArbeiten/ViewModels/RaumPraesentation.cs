namespace MaturaArbeiten.ViewModels;

public sealed record RaumPraesentation(
    int Id,
    string Thema,
    DateOnly Datum,
    TimeOnly Uhrzeit,
    IReadOnlyList<string> Zuseher);
