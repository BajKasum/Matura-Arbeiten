using System.Text;
using MaturaArbeiten.Domain;
using Microsoft.VisualBasic.FileIO;

namespace MaturaArbeiten.Services;

public static class CsvLeser
{
    public static IReadOnlyList<CsvZeile> Lesen(Stream datei)
    {
        using var leser = new StreamReader(datei, Encoding.UTF8, true);

        return leser.ReadToEnd()
            .Split('\n')
            .Select((text, index) => (Nummer: index + 1, Text: text.TrimEnd('\r')))
            .Where(zeile => zeile.Text.Length > 0)
            .Select(zeile => new CsvZeile(zeile.Nummer, Felder(zeile.Text)))
            .ToList();
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