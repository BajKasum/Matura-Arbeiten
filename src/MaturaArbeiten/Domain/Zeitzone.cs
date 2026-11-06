namespace MaturaArbeiten.Domain;

public static class Zeitzone
{
    private static readonly TimeZoneInfo Zuerich = TimeZoneInfo.FindSystemTimeZoneById("Europe/Zurich");

    public static DateTime InZuerich(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Zuerich);
}
