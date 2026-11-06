namespace MaturaArbeiten.ViewModels;

public sealed record PraesentationZeile(
    int Id,
    string Thema,
    DateOnly Datum,
    TimeOnly Uhrzeit,
    string Raum,
    int FreiePlaetze,
    bool Gewaehlt);