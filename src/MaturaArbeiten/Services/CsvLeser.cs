using System.Text;
using MaturaArbeiten.Domain;
using Microsoft.VisualBasic.FileIO;

namespace MaturaArbeiten.Services;

public static class CsvLeser
{
    private const string KeinUtf8 =
        "Die Datei wurde nicht geladen, weil sie nicht als UTF-8 gespeichert ist. " +
        "Speichere sie in Excel als \"CSV UTF-8 (durch Trennzeichen getrennt)\" und lade sie noch einmal.";

    private static readonly UTF8Encoding StrengesUtf8 = new(false, true);

    public static CsvLesung Lesen(Stream datei)
    {
        try
        {
            using var leser = new StreamReader(datei, StrengesUtf8, true);

            var zeilen = leser.ReadToEnd()
                .Split('\n')
                .Select((text, index) => (Nummer: index + 1, Text: text.TrimEnd('\r')))
                .Where(zeile => zeile.Text.Length > 0)
                .Select(zeile => new CsvZeile(zeile.Nummer, Felder(zeile.Text)))
                .ToList();

            return new CsvLesung(zeilen, null);
        }
        catch (DecoderFallbackException)
        {
            return new CsvLesung([], KeinUtf8);
        }
    }

    private static string[] Felder(string text)
    {
        using var parser = new TextFieldParser(new StringReader(text))
        {
            TextFieldType = FieldType.Delimited,
            HasFieldsEnclosedInQuotes = true
        };
        parser.SetDelimiters(";");

        try
        {
            return parser.ReadFields() ?? [];
        }
        catch (MalformedLineException)
        {
            return [];
        }
    }
}
