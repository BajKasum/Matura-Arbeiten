namespace MaturaArbeiten.Domain;

public sealed record PraesentationNeu(string Thema, DateOnly Datum, TimeOnly Uhrzeit, string Raum);