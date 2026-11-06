namespace MaturaArbeiten.Domain;

public sealed record MailEintragung(string Thema, DateOnly Datum, TimeOnly Uhrzeit, string Raum);
