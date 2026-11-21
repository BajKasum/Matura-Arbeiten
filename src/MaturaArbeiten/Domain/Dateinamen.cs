using System.Text.RegularExpressions;

namespace MaturaArbeiten.Domain;

public static partial class Dateinamen
{
    private const int MaxLaenge = 60;
    private const string Ersatz = "Praesentation";

    public static string Aus(string text)
    {
        var wort = string.Join("_", WoerterRegex().Matches(text).Select(treffer => treffer.Value));
        var gekuerzt = wort.Length > MaxLaenge ? wort[..MaxLaenge].TrimEnd('_') : wort;

        return gekuerzt.Length == 0 ? Ersatz : gekuerzt;
    }

    [GeneratedRegex(@"[\p{L}\p{N}]+")]
    private static partial Regex WoerterRegex();
}
