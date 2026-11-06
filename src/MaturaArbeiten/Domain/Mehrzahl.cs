namespace MaturaArbeiten.Domain;

public static class Mehrzahl
{
    public static string Von(int anzahl, string einzahl, string mehrzahl) =>
        $"{anzahl} {(anzahl == 1 ? einzahl : mehrzahl)}";
}
