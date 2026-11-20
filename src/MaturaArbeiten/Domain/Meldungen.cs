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
        Abweisung.Konflikt => "Gerade wählen viele gleichzeitig. Deine Eintragung wurde nicht gespeichert, bitte versuche es noch einmal.",
        Abweisung.PraesentationUnbekannt => "Diese Präsentation gibt es nicht. Bitte wähle eine andere.",
        Abweisung.G3SuSUnbekannt => "Dein Konto gibt es nicht mehr. Bitte melde dich bei der Administration.",
        _ => throw new ArgumentOutOfRangeException(nameof(grund))
    };

    public static string FuerAdministration(Abweisung grund) => grund switch
    {
        Abweisung.PhaseGeschlossen => "Die Eintragungsphase ist geschlossen.",
        Abweisung.SchonZweiEintragungen => "Dieser G3-SuS hat schon zwei Eintragungen.",
        Abweisung.PraesentationVoll => "Diese Präsentation ist voll.",
        Abweisung.Terminkollision => "Dieser G3-SuS ist schon bei einer Präsentation zur gleichen Zeit eingetragen.",
        Abweisung.Konflikt => "Gerade wird gleichzeitig eingetragen. Die Eintragung wurde nicht gespeichert, bitte versuche es noch einmal.",
        Abweisung.PraesentationUnbekannt => "Diese Präsentation gibt es nicht.",
        Abweisung.G3SuSUnbekannt => "Diesen G3-SuS gibt es nicht.",
        _ => throw new ArgumentOutOfRangeException(nameof(grund))
    };

    public static string Gesperrt(DateTime bis, DateTime jetzt) =>
        Math.Max(1, (int)Math.Ceiling((bis - jetzt).TotalMinutes)) switch
        {
            1 => "Zu viele falsche Versuche. Bitte warte noch eine Minute.",
            var minuten => $"Zu viele falsche Versuche. Bitte warte noch {minuten} Minuten."
        };
}
