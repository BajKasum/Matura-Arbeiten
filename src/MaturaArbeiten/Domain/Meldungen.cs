namespace MaturaArbeiten.Domain;

public static class Meldungen
{
    public const string AnmeldungFalsch = "E-Mail-Adresse oder Passwort stimmt nicht.";

    public static string Fuer(Abweisung grund) => grund switch
    {
        Abweisung.PhaseGeschlossen => "Die Eintragungsphase ist geschlossen.",
        Abweisung.SchonZweiEintragungen => "Du hast schon zwei Präsentationen gewählt.",
        Abweisung.PraesentationVoll => "Diese Präsentation ist inzwischen voll. Bitte wähle eine andere.",
        Abweisung.Terminkollision => "Diese Präsentation läuft gleichzeitig mit einer, die du schon gewählt hast.",
        _ => throw new ArgumentOutOfRangeException(nameof(grund))
    };

    public static string Gesperrt(DateTime bis, DateTime jetzt) =>
        Math.Max(1, (int)Math.Ceiling((bis - jetzt).TotalMinutes)) switch
        {
            1 => "Zu viele falsche Versuche. Bitte warte noch eine Minute.",
            var minuten => $"Zu viele falsche Versuche. Bitte warte noch {minuten} Minuten."
        };
}