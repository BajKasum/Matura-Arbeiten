namespace MaturaArbeiten.Domain;

public static class Passwoerter
{
    public const int MindestLaenge = 10;

    public static string? Pruefe(string aktuell, string neu, string wiederholung) =>
        neu.Length < MindestLaenge
            ? $"Das neue Passwort muss mindestens {MindestLaenge} Zeichen lang sein."
            : neu != wiederholung
                ? "Die beiden neuen Passwörter sind nicht gleich."
                : neu == aktuell
                    ? "Das neue Passwort muss sich vom bisherigen unterscheiden."
                    : null;
}
