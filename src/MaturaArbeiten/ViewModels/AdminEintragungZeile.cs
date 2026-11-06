namespace MaturaArbeiten.ViewModels;

public sealed record AdminEintragungZeile(
    int Id,
    string G3SuS,
    string Thema,
    DateOnly Datum,
    TimeOnly Uhrzeit,
    string Raum,
    DateTime ErstelltAm);
