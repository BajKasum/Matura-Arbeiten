namespace MaturaArbeiten.Domain;

public abstract record AnmeldeErgebnis
{
    private AnmeldeErgebnis()
    {
    }

    public sealed record Erfolg(int Id, string Name, string Rolle, bool PasswortAendern) : AnmeldeErgebnis;

    public sealed record Gesperrt(DateTime Bis) : AnmeldeErgebnis;

    public sealed record Abgewiesen : AnmeldeErgebnis;
}
