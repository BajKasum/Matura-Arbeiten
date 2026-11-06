using MaturaArbeiten.Data;
using MaturaArbeiten.Services;

namespace MaturaArbeiten.Tests;

[TestClass]
public class ImportServiceTests
{
    private readonly AppDbContext _db = TestDb.Neu();

    private ImportService Dienst() => new(_db);

    [TestMethod]
    public void Praesentationen_werden_geladen_und_Fehler_mit_Zeilennummer_gemeldet()
    {
        var datei = TestDb.Csv(
            "Thema;Datum;Uhrzeit;Raum\r\n"
            + "Klimawandel;12.01.2027;08:30;204\r\n"
            + "Kaputt;kein Datum;08:30;204\r\n"
            + "Fairtrade;13.01.2027;09:30;302\r\n");

        var ergebnis = Dienst().PraesentationenLaden(datei);

        Assert.AreEqual("Präsentationen", ergebnis.Titel);
        Assert.AreEqual(2, ergebnis.Geladen);
        Assert.AreEqual(3, ergebnis.Fehler.Single().Zeile);
        Assert.AreEqual(2, _db.Praesentationen.Count());
        Assert.AreEqual(new TimeOnly(8, 30), _db.Praesentationen.Single(p => p.Thema == "Klimawandel").Uhrzeit);
    }

    [TestMethod]
    public void G3SuS_werden_geladen_ohne_Passwort()
    {
        var datei = TestDb.Csv("Vorname;Nachname;E-Mail\r\nAnna;Muster;anna@stud.edubs.ch\r\n");

        var ergebnis = Dienst().G3SuSLaden(datei);

        Assert.AreEqual("G3-SuS", ergebnis.Titel);
        Assert.AreEqual(1, ergebnis.Geladen);
        Assert.IsNull(_db.G3SuSListe.Single().PasswortHash);
    }

    [TestMethod]
    public void G3SuS_mit_bereits_vorhandener_Adresse_werden_uebersprungen_auch_bei_Administrationslogins()
    {
        _db.Schueler("Anna", "Muster");
        _db.Admin("Ben", "Lehrer");
        var datei = TestDb.Csv(
            "Vorname;Nachname;E-Mail\r\n"
            + "Anna;Neu;ANNA.MUSTER@stud.edubs.ch\r\n"
            + "Ben;Neu;ben.lehrer@schule.ch\r\n"
            + "Clara;Neu;clara@stud.edubs.ch\r\n");

        var ergebnis = Dienst().G3SuSLaden(datei);

        Assert.AreEqual(1, ergebnis.Geladen);
        Assert.HasCount(2, ergebnis.Fehler);
        Assert.AreEqual(2, _db.G3SuSListe.Count());
    }

    [TestMethod]
    public void Eine_Datei_mit_nur_dem_Kopf_laedt_nichts()
    {
        var ergebnis = Dienst().PraesentationenLaden(TestDb.Csv("Thema;Datum;Uhrzeit;Raum\r\n"));

        Assert.AreEqual(0, ergebnis.Geladen);
        Assert.IsEmpty(ergebnis.Fehler);
    }
}
