namespace MaturaArbeiten.Domain;

public static class Sperre
{
    public const int MaxFehlversuche = 5;
    public static readonly TimeSpan Dauer = TimeSpan.FromMinutes(15);

    public static bool IstGesperrt(Konto konto, DateTime jetzt) => konto.GesperrtBis > jetzt;

    // Der letzte erlaubte Fehlversuch löst die Sperre aus und setzt den Zähler auf 0, nach der Sperre gibt es wieder volle Versuche.
    public static Konto NachFalschemPasswort(Konto konto, DateTime jetzt) =>
        konto.Fehlversuche + 1 >= MaxFehlversuche
            ? new Konto(0, jetzt + Dauer)
            : new Konto(konto.Fehlversuche + 1, null);

    public static Konto NachRichtigemPasswort() => new(0, null);
}