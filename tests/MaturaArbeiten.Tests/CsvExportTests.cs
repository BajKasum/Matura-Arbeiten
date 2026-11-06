using MaturaArbeiten.Domain;

namespace MaturaArbeiten.Tests;

[TestClass]
public class CsvExportTests
{
    [TestMethod]
    public void Zeile_verbindet_die_Felder_mit_Strichpunkt()
    {
        Assert.AreEqual("a;b;c", CsvExport.Zeile(["a", "b", "c"]));
    }

    [TestMethod]
    public void Zeile_setzt_Felder_mit_Sonderzeichen_in_Anfuehrungszeichen()
    {
        Assert.AreEqual("\"a;b\"", CsvExport.Zeile(["a;b"]));
        Assert.AreEqual("\"sagt \"\"hallo\"\"\"", CsvExport.Zeile(["sagt \"hallo\""]));
        Assert.AreEqual("\"eins\ntwo\"", CsvExport.Zeile(["eins\ntwo"]));
    }

    [TestMethod]
    [DataRow("=SUMME(A1)", "'=SUMME(A1)")]
    [DataRow("+41", "'+41")]
    [DataRow("-5", "'-5")]
    [DataRow("@name", "'@name")]
    public void Zeile_schuetzt_Felder_die_wie_eine_Formel_beginnen(string eingabe, string erwartet)
    {
        Assert.AreEqual(erwartet, CsvExport.Zeile([eingabe]));
    }

    [TestMethod]
    public void Zeile_laesst_normale_Felder_und_leere_Felder_unveraendert()
    {
        Assert.AreEqual("Anna;;204", CsvExport.Zeile(["Anna", "", "204"]));
    }

    [TestMethod]
    public void Datei_beendet_jede_Zeile_mit_Wagenruecklauf_und_Zeilenvorschub()
    {
        Assert.AreEqual("a;b\r\nc;d\r\n", CsvExport.Datei([["a", "b"], ["c", "d"]]));
    }

    [TestMethod]
    public void Datei_ohne_Zeilen_ist_leer()
    {
        Assert.AreEqual(string.Empty, CsvExport.Datei([]));
    }
}
