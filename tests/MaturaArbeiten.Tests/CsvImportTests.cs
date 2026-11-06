using MaturaArbeiten.Domain;

namespace MaturaArbeiten.Tests;

[TestClass]
public class CsvImportTests
{
    private static CsvZeile Kopf { get; } = new(1, ["Kopf"]);

    private static CsvZeile Z(int nummer, params string[] felder) => new(nummer, felder);

    private static string GrundVon(CsvErgebnis<PraesentationNeu> ergebnis) => ergebnis.Fehler.Single().Grund;

    [TestMethod]
    public void Praesentationen_eine_gueltige_Zeile_wird_uebernommen_und_der_Kopf_uebersprungen()
    {
        var ergebnis = CsvImport.PraesentationenPruefen([Kopf, Z(2, "Klimawandel", "12.01.2027", "08:30", "204")]);

        Assert.IsEmpty(ergebnis.Fehler);
        Assert.AreEqual(
            new PraesentationNeu("Klimawandel", new DateOnly(2027, 1, 12), new TimeOnly(8, 30), "204"),
            ergebnis.Gueltig.Single());
    }

    [TestMethod]
    public void Praesentationen_kurze_Schreibweise_fuer_Datum_und_Uhrzeit_ist_erlaubt()
    {
        var ergebnis = CsvImport.PraesentationenPruefen([Kopf, Z(2, "Thema", "2.3.2027", "8:05", "A1")]);

        Assert.AreEqual(new DateOnly(2027, 3, 2), ergebnis.Gueltig.Single().Datum);
        Assert.AreEqual(new TimeOnly(8, 5), ergebnis.Gueltig.Single().Uhrzeit);
    }

    [TestMethod]
    public void Praesentationen_Leerzeichen_um_die_Felder_werden_entfernt()
    {
        var ergebnis = CsvImport.PraesentationenPruefen([Kopf, Z(2, " Thema ", " 12.01.2027 ", " 08:30 ", " 204 ")]);

        Assert.AreEqual("Thema", ergebnis.Gueltig.Single().Thema);
        Assert.AreEqual("204", ergebnis.Gueltig.Single().Raum);
    }

    [TestMethod]
    public void Praesentationen_falsche_Spaltenzahl_wird_gemeldet()
    {
        var ergebnis = CsvImport.PraesentationenPruefen([Kopf, Z(2, "Thema", "12.01.2027", "08:30")]);

        Assert.IsEmpty(ergebnis.Gueltig);
        Assert.AreEqual(2, ergebnis.Fehler.Single().Zeile);
        Assert.AreEqual("Die Zeile hat nicht genau vier Spalten.", GrundVon(ergebnis));
    }

    [TestMethod]
    [DataRow("", "12.01.2027", "08:30", "204", "Thema fehlt oder ist länger als 200 Zeichen.")]
    [DataRow("Thema", "12.01.2027", "08:30", "", "Raum fehlt oder ist länger als 20 Zeichen.")]
    [DataRow("Thema", "2027-01-12", "08:30", "204", "Datum ist nicht im Format TT.MM.JJJJ.")]
    [DataRow("Thema", "32.01.2027", "08:30", "204", "Datum ist nicht im Format TT.MM.JJJJ.")]
    [DataRow("Thema", "12.01.2027", "25:00", "204", "Uhrzeit ist nicht im Format HH:MM.")]
    [DataRow("Thema", "12.01.2027", "acht", "204", "Uhrzeit ist nicht im Format HH:MM.")]
    public void Praesentationen_ungueltige_Felder_werden_mit_Grund_gemeldet(
        string thema,
        string datum,
        string uhrzeit,
        string raum,
        string erwartet)
    {
        var ergebnis = CsvImport.PraesentationenPruefen([Kopf, Z(2, thema, datum, uhrzeit, raum)]);

        Assert.IsEmpty(ergebnis.Gueltig);
        Assert.AreEqual(erwartet, GrundVon(ergebnis));
    }

    [TestMethod]
    public void Praesentationen_Laengengrenzen_gelten_genau()
    {
        var gut = CsvImport.PraesentationenPruefen([Kopf, Z(2, new string('a', 200), "12.01.2027", "08:30", new string('r', 20))]);
        var zuLang = CsvImport.PraesentationenPruefen([Kopf, Z(2, new string('a', 201), "12.01.2027", "08:30", "204")]);
        var raumZuLang = CsvImport.PraesentationenPruefen([Kopf, Z(2, "Thema", "12.01.2027", "08:30", new string('r', 21))]);

        Assert.HasCount(1, gut.Gueltig);
        Assert.HasCount(1, zuLang.Fehler);
        Assert.HasCount(1, raumZuLang.Fehler);
    }

    [TestMethod]
    public void Praesentationen_gueltige_und_fehlerhafte_Zeilen_werden_getrennt()
    {
        var ergebnis = CsvImport.PraesentationenPruefen(
        [
            Kopf,
            Z(2, "Eins", "12.01.2027", "08:30", "204"),
            Z(3, "Zwei", "kein Datum", "08:30", "204"),
            Z(5, "Drei", "13.01.2027", "09:30", "302")
        ]);

        CollectionAssert.AreEqual(new[] { "Eins", "Drei" }, ergebnis.Gueltig.Select(p => p.Thema).ToArray());
        Assert.AreEqual(3, ergebnis.Fehler.Single().Zeile);
    }

    [TestMethod]
    public void G3SuS_eine_gueltige_Zeile_wird_uebernommen()
    {
        var ergebnis = CsvImport.G3SuSPruefen([Kopf, Z(2, "Anna", "Muster", "anna.muster@stud.edubs.ch")], []);

        Assert.IsEmpty(ergebnis.Fehler);
        Assert.AreEqual(new G3SuSNeu("Anna", "Muster", "anna.muster@stud.edubs.ch"), ergebnis.Gueltig.Single());
    }

    [TestMethod]
    public void G3SuS_doppelte_Adresse_in_der_Datei_wird_erst_ab_der_zweiten_gemeldet()
    {
        var ergebnis = CsvImport.G3SuSPruefen(
            [Kopf, Z(2, "Anna", "Muster", "a@x.ch"), Z(3, "Anna", "Zwei", "A@X.ch")],
            []);

        Assert.HasCount(1, ergebnis.Gueltig);
        Assert.AreEqual(3, ergebnis.Fehler.Single().Zeile);
        Assert.AreEqual("Diese E-Mail-Adresse ist doppelt.", ergebnis.Fehler.Single().Grund);
    }

    [TestMethod]
    public void G3SuS_bereits_vorhandene_Adresse_wird_ohne_Beachtung_der_Gross_und_Kleinschreibung_abgewiesen()
    {
        var ergebnis = CsvImport.G3SuSPruefen([Kopf, Z(2, "Anna", "Muster", "ANNA@x.ch")], ["anna@x.ch"]);

        Assert.IsEmpty(ergebnis.Gueltig);
        Assert.AreEqual("Diese E-Mail-Adresse ist doppelt.", ergebnis.Fehler.Single().Grund);
    }

    [TestMethod]
    public void G3SuS_falsche_Spaltenzahl_wird_gemeldet()
    {
        var ergebnis = CsvImport.G3SuSPruefen([Kopf, Z(2, "Anna", "Muster")], []);

        Assert.AreEqual("Die Zeile hat nicht genau drei Spalten.", ergebnis.Fehler.Single().Grund);
    }

    [TestMethod]
    [DataRow("", "Muster", "a@x.ch", "Vorname fehlt oder ist länger als 50 Zeichen.")]
    [DataRow("Anna", "", "a@x.ch", "Nachname fehlt oder ist länger als 50 Zeichen.")]
    [DataRow("Anna", "Muster", "", "E-Mail-Adresse fehlt oder ist länger als 100 Zeichen.")]
    [DataRow("Anna", "Muster", "keine-adresse", "Die E-Mail-Adresse ist nicht gültig.")]
    [DataRow("Anna", "Muster", "Anna Muster <a@x.ch>", "Die E-Mail-Adresse ist nicht gültig.")]
    public void G3SuS_ungueltige_Felder_werden_mit_Grund_gemeldet(
        string vorname,
        string nachname,
        string email,
        string erwartet)
    {
        var ergebnis = CsvImport.G3SuSPruefen([Kopf, Z(2, vorname, nachname, email)], []);

        Assert.IsEmpty(ergebnis.Gueltig);
        Assert.AreEqual(erwartet, ergebnis.Fehler.Single().Grund);
    }
}
