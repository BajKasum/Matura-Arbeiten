namespace MaturaArbeiten.ViewModels;

public sealed record EintragungZeile(
    string Thema,
    DateOnly Datum,
    TimeOnly Uhrzeit,
    string Raum);