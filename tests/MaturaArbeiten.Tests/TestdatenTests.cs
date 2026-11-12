using MaturaArbeiten.Data;
using MaturaArbeiten.Domain;
using MaturaArbeiten.Services;

namespace MaturaArbeiten.Tests;

[TestClass]
public class TestdatenTests
{
    private const string VierSpalten = "Die Zeile hat nicht genau vier Spalten.";
    private const string DreiSpalten = "Die Zeile hat nicht genau drei Spalten.";
    private const string Thema = "Thema fehlt oder ist länger als 200 Zeichen.";
    private const string Raum = "Raum fehlt oder ist länger als 20 Zeichen.";
    private const string Datum = "Datum ist nicht im Format TT.MM.JJJJ.";
    private const string Uhrzeit = "Uhrzeit ist nicht im Format HH:MM.";
    private const string Vorname = "Vorname fehlt oder ist länger als 50 Zeichen.";
    private const string Nachname = "Nachname fehlt oder ist länger als 50 Zeichen.";
    private const string EmailLaenge = "E-Mail-Adresse fehlt oder ist länger als 100 Zeichen.";
    private const string EmailUngueltig = "Die E-Mail-Adresse ist nicht gültig.";
    private const string EmailDoppelt = "Diese E-Mail-Adresse ist doppelt.";

    private readonly AppDbContext _db = TestDb.Neu();

    private static FileStream Datei(string name) =>
        File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Testdaten", name));

    private static IEnumerable<CsvFehler> Fehler(string grund, params int[] zeilen) =>
        zeilen.Select(zeile => new CsvFehler(zeile, grund));

    private static IEnumerable<CsvFehler> Bereich(string grund, int von, int bis) =>
        Fehler(grund, [.. Enumerable.Range(von, bis - von + 1)]);

    [TestMethod]
    public void Praesentationen_Testdaten_ergeben_zwanzig_gueltige_Zeilen_und_die_erwarteten_Fehler()
    {
        var ergebnis = CsvImport.PraesentationenPruefen(CsvLeser.Lesen(Datei("Praesentationen_Testdaten.csv")));

        Assert.HasCount(20, ergebnis.Gueltig);
        var erwartet = Fehler(VierSpalten, 23, 24)
            .Concat(Fehler(Thema, 25, 26))
            .Concat(Fehler(Raum, 27, 28))
            .Concat(Bereich(Datum, 29, 35))
            .Concat(Bereich(Uhrzeit, 36, 40))
            .Concat(Fehler(VierSpalten, 41, 42))
            .ToList();
        CollectionAssert.AreEqual(erwartet, ergebnis.Fehler.ToList());
    }

    [TestMethod]
    public void Praesentationen_Testdaten_Sonderfaelle_werden_richtig_gelesen()
    {
        var gueltig = CsvImport.PraesentationenPruefen(CsvLeser.Lesen(Datei("Praesentationen_Testdaten.csv"))).Gueltig;

        Assert.AreEqual(new DateOnly(2027, 1, 5), gueltig.Single(p => p.Thema == "Testfall kurzes Datum und kurze Uhrzeit").Datum);
        Assert.AreEqual(new TimeOnly(8, 5), gueltig.Single(p => p.Thema == "Testfall kurzes Datum und kurze Uhrzeit").Uhrzeit);
        Assert.AreEqual(new TimeOnly(10, 30), gueltig.Single(p => p.Thema == "Testfall Uhrzeit mit Sekunden").Uhrzeit);
        Assert.AreEqual("C3", gueltig.Single(p => p.Thema == "Testfall Leerzeichen um die Felder").Raum);
        Assert.IsTrue(gueltig.Any(p => p.Thema == "Testfall: Strichpunkt; im Thema"));
        Assert.IsTrue(gueltig.Any(p => p.Thema == "Testfall \"Anführungszeichen\" im Thema"));
        Assert.IsTrue(gueltig.Any(p => p.Thema.StartsWith("Testfall Sonderzeichen: Äpfel, Öl & Übergrössen – Zürich «Ärger»")));
        Assert.AreEqual(200, gueltig.Max(p => p.Thema.Length));
        Assert.AreEqual(20, gueltig.Max(p => p.Raum.Length));
        Assert.AreEqual(new DateOnly(2028, 2, 29), gueltig.Single(p => p.Thema == "Testfall Schalttag").Datum);
    }

    [TestMethod]
    public void G3SuS_Testdaten_ergeben_achtundzwanzig_gueltige_Zeilen_und_die_erwarteten_Fehler()
    {
        var ergebnis = CsvImport.G3SuSPruefen(CsvLeser.Lesen(Datei("G3SuS_Testdaten.csv")), []);

        Assert.HasCount(28, ergebnis.Gueltig);
        var erwartet = Fehler(DreiSpalten, 31, 32)
            .Concat(Fehler(Vorname, 33, 36))
            .Concat(Fehler(Nachname, 34, 37))
            .Concat(Fehler(EmailLaenge, 35, 38))
            .Concat(Bereich(EmailUngueltig, 39, 44))
            .Concat(Bereich(EmailDoppelt, 45, 47))
            .Concat(Fehler(DreiSpalten, 48, 49))
            .OrderBy(f => f.Zeile)
            .ToList();
        CollectionAssert.AreEqual(erwartet, ergebnis.Fehler.ToList());
    }

    [TestMethod]
    public void G3SuS_Testdaten_Sonderfaelle_werden_richtig_gelesen()
    {
        var gueltig = CsvImport.G3SuSPruefen(CsvLeser.Lesen(Datei("G3SuS_Testdaten.csv")), []).Gueltig;

        Assert.AreEqual(new G3SuSNeu("Lea", "Zimmermann", "lea.zimmermann@stud.edubs.ch"), gueltig.Single(g => g.Vorname == "Lea"));
        Assert.AreEqual("Mia.Gross@Stud.Edubs.ch", gueltig.Single(g => g.Vorname == "Mia").Email);
        Assert.AreEqual("O'Brien", gueltig.Single(g => g.Vorname == "Zoé").Nachname);
        Assert.AreEqual("El-Amin", gueltig.Single(g => g.Vorname == "Yara").Nachname);
        Assert.AreEqual(100, gueltig.Max(g => g.Email.Length));
        Assert.AreEqual(50, gueltig.Max(g => g.Vorname.Length));
        Assert.AreEqual(50, gueltig.Max(g => g.Nachname.Length));
    }

    [TestMethod]
    public void G3SuS_Testdaten_zweimal_geladen_nehmen_beim_zweiten_Mal_niemanden_mehr_auf()
    {
        var dienst = new ImportService(_db);

        var erstes = dienst.G3SuSLaden(Datei("G3SuS_Testdaten.csv"));
        var zweites = dienst.G3SuSLaden(Datei("G3SuS_Testdaten.csv"));

        Assert.AreEqual(28, erstes.Geladen);
        Assert.HasCount(19, erstes.Fehler);
        Assert.AreEqual(0, zweites.Geladen);
        Assert.HasCount(47, zweites.Fehler);
        Assert.AreEqual(28, _db.G3SuSListe.Count());
    }

    [TestMethod]
    public void Praesentationen_Testdaten_zweimal_geladen_legen_die_Praesentationen_doppelt_an()
    {
        var dienst = new ImportService(_db);

        dienst.PraesentationenLaden(Datei("Praesentationen_Testdaten.csv"));
        dienst.PraesentationenLaden(Datei("Praesentationen_Testdaten.csv"));

        Assert.AreEqual(40, _db.Praesentationen.Count());
    }

    [TestMethod]
    public void Eine_Formel_als_Thema_wird_in_der_CSV_Liste_mit_Hochkomma_geschuetzt()
    {
        new ImportService(_db).PraesentationenLaden(Datei("Praesentationen_Testdaten.csv"));

        var csv = new ListenService(_db).RaumuebersichtCsv();

        Assert.Contains("'=1+1 Testfall Formelschutz im Export", csv);
    }

    [TestMethod]
    public void Mit_den_Testdaten_lassen_sich_sechs_G3SuS_eintragen_und_der_siebte_wird_abgewiesen()
    {
        var import = new ImportService(_db);
        import.PraesentationenLaden(Datei("Praesentationen_Testdaten.csv"));
        import.G3SuSLaden(Datei("G3SuS_Testdaten.csv"));
        var dienst = new EintragungService(_db, new TestZeit());
        var klima = _db.Praesentationen.Single(p => p.Thema.StartsWith("Klimawandel"));
        var schueler = _db.G3SuSListe.OrderBy(g => g.Id).Take(7).ToList();

        var ergebnisse = schueler.Select(g => dienst.Eintragen(g.Id, klima.Id)).ToList();

        Assert.AreEqual(6, ergebnisse.Count(e => e is EintragungErgebnis.Erfolg));
        Assert.AreEqual(
            new EintragungErgebnis.Abgewiesen(Abweisung.PraesentationVoll),
            ergebnisse[6]);
    }
}
