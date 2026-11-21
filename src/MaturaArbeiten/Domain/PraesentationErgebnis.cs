namespace MaturaArbeiten.Domain;

public abstract record PraesentationErgebnis
{
    private PraesentationErgebnis()
    {
    }

    public sealed record Erfolg(int BetroffeneEintragungen) : PraesentationErgebnis;

    public sealed record Abgewiesen(string Grund) : PraesentationErgebnis;
}
