namespace MaturaArbeiten.Domain;

public static class CsvExport
{
    private const string FormelZeichen = "=+-@\t\r";

    public static string Datei(IEnumerable<IEnumerable<string>> zeilen) =>
        string.Concat(zeilen.Select(zeile => Zeile(zeile) + "\r\n"));

    public static string Zeile(IEnumerable<string> felder) =>
        string.Join(";", felder.Select(Feld));

    private static string Feld(string wert)
    {
        var sicher = wert.Length > 0 && FormelZeichen.Contains(wert[0]) ? "'" + wert : wert;

        return sicher.IndexOfAny([';', '"', '\r', '\n']) >= 0
            ? "\"" + sicher.Replace("\"", "\"\"") + "\""
            : sicher;
    }
}
