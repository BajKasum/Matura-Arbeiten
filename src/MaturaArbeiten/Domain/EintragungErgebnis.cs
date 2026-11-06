namespace MaturaArbeiten.Domain;

public abstract record EintragungErgebnis
{
    private EintragungErgebnis()
    {
    }

    public sealed record Erfolg : EintragungErgebnis;

    public sealed record Abgewiesen(Abweisung Grund) : EintragungErgebnis;
}