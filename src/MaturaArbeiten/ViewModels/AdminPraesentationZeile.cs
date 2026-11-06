namespace MaturaArbeiten.ViewModels;

public sealed record AdminPraesentationZeile(
    int Id,
    string Thema,
    DateOnly Datum,
    TimeOnly Uhrzeit,
    string Raum,
    int Zuseher,
    int FreiePlaetze);
