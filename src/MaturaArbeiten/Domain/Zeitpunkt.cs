namespace MaturaArbeiten.Domain;

public readonly record struct Zeitpunkt(DateOnly Datum, TimeOnly Uhrzeit);