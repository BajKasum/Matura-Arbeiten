using System.Text;
using MaturaArbeiten.Domain;
using MaturaArbeiten.Services;

namespace MaturaArbeiten.Tests;

[TestClass]
public class CsvLeserTests
{
    [TestMethod]
    public void Lesen_trennt_die_Felder_am_Strichpunkt_und_nummeriert_die_Zeilen()
    {
        var zeilen = CsvLeser.Lesen(TestDb.Csv("Kopf1;Kopf2\r\nA;B\r\nC;D\r\n"));

        Assert.HasCount(3, zeilen);
        Assert.AreEqual(1, zeilen[0].Nummer);
        CollectionAssert.AreEqual(new[] { "A", "B" }, zeilen[1].Felder);
        Assert.AreEqual(3, zeilen[2].Nummer);
    }

    [TestMethod]
    public void Lesen_ueberspringt_leere_Zeilen_zaehlt_sie_aber_mit()
    {
        var zeilen = CsvLeser.Lesen(TestDb.Csv("Kopf\r\nA\r\n\r\nB\r\n"));

        CollectionAssert.AreEqual(new[] { 1, 2, 4 }, zeilen.Select(z => z.Nummer).ToArray());
    }

    [TestMethod]
    public void Lesen_funktioniert_auch_ohne_BOM_und_mit_Zeilenende_LF()
    {
        var zeilen = CsvLeser.Lesen(new MemoryStream(Encoding.UTF8.GetBytes("Kopf\nÄ;Ö\n")));

        CollectionAssert.AreEqual(new[] { "Ä", "Ö" }, zeilen[1].Felder);
    }

    [TestMethod]
    public void Lesen_entfernt_das_BOM_aus_dem_ersten_Feld()
    {
        var zeilen = CsvLeser.Lesen(TestDb.Csv("Vorname;Nachname\r\n"));

        Assert.AreEqual("Vorname", zeilen[0].Felder[0]);
    }

    [TestMethod]
    public void Lesen_versteht_Anfuehrungszeichen_mit_Strichpunkt_im_Feld()
    {
        var zeilen = CsvLeser.Lesen(TestDb.Csv("Kopf\r\n\"Klima; Wandel\";12.01.2027\r\n"));

        CollectionAssert.AreEqual(new[] { "Klima; Wandel", "12.01.2027" }, zeilen[1].Felder);
    }

    [TestMethod]
    public void Lesen_liefert_bei_kaputten_Anfuehrungszeichen_eine_Zeile_ohne_Felder()
    {
        var zeilen = CsvLeser.Lesen(TestDb.Csv("Kopf\r\nA;\"B\r\n"));

        Assert.IsEmpty(zeilen[1].Felder);
    }

    [TestMethod]
    public void Lesen_einer_leeren_Datei_liefert_keine_Zeilen()
    {
        Assert.IsEmpty(CsvLeser.Lesen(new MemoryStream()));
    }
}
