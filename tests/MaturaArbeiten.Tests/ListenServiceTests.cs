using MaturaArbeiten.Data;
using MaturaArbeiten.Services;

namespace MaturaArbeiten.Tests;

[TestClass]
public class ListenServiceTests
{
    private readonly AppDbContext _db = TestDb.Neu();

    private ListenService Dienst() => new(_db);

    [TestMethod]
    public void Raumuebersicht_gruppiert_nach_Raum_und_sortiert_Praesentationen_nach_Zeit()
    {
        var anna = _db.Schueler("Anna", "Muster");
        var ben = _db.Schueler("Ben", "Beispiel");
        var spaet = _db.NeuePraesentation("Spaet", 13, 8, "204");
        _db.NeuePraesentation("Frueh", 12, 8, "204");
        var anderer = _db.NeuePraesentation("Anderer Raum", 12, 8, "102");
        _db.Eintragen(anna, anderer);
        _db.Eintragen(ben, anderer);
        _db.Eintragen(anna, spaet);

        var raeume = Dienst().Raumuebersicht().Raeume;

        CollectionAssert.AreEqual(new[] { "102", "204" }, raeume.Select(r => r.Raum).ToArray());
        CollectionAssert.AreEqual(
            new[] { "Frueh", "Spaet" },
            raeume[1].Praesentationen.Select(p => p.Thema).ToArray());
        CollectionAssert.AreEqual(
            new[] { "Beispiel Ben", "Muster Anna" },
            raeume[0].Praesentationen.Single().Zuseher.ToArray());
    }

    [TestMethod]
    public void Raumuebersicht_ohne_Praesentationen_ist_leer()
    {
        Assert.IsEmpty(Dienst().Raumuebersicht().Raeume);
    }

    [TestMethod]
    public void RaumuebersichtCsv_hat_Kopfzeile_und_je_Praesentation_eine_Zeile()
    {
        var anna = _db.Schueler("Anna", "Muster");
        _db.Eintragen(anna, _db.NeuePraesentation("Klimawandel", 12, 8, "204"));

        var csv = Dienst().RaumuebersichtCsv();

        Assert.AreEqual(
            "Raum;Datum;Uhrzeit;Thema;Zuseher\r\n204;12.01.2027;08:00;Klimawandel;Muster Anna\r\n",
            csv);
    }

    [TestMethod]
    public void Zuseherliste_ohne_Auswahl_nimmt_die_erste_Praesentation()
    {
        var anna = _db.Schueler("Anna", "Muster");
        _db.NeuePraesentation("Spaet", 13, 8);
        var frueh = _db.NeuePraesentation("Frueh", 12, 8);
        _db.Eintragen(anna, frueh);

        var liste = Dienst().Zuseherliste(null)!;

        Assert.HasCount(2, liste.Praesentationen);
        Assert.AreEqual("Frueh", liste.Gewaehlt!.Thema);
        Assert.AreEqual(1, liste.Gewaehlt.Zuseher);
        Assert.AreEqual(5, liste.Gewaehlt.FreiePlaetze);
        Assert.AreEqual("Anna", liste.Zuseher.Single().Vorname);
        Assert.AreEqual(anna.Email, liste.Zuseher.Single().Email);
    }

    [TestMethod]
    public void Zuseherliste_ohne_Praesentationen_hat_keine_Auswahl()
    {
        var liste = Dienst().Zuseherliste(null)!;

        Assert.IsNull(liste.Gewaehlt);
        Assert.IsEmpty(liste.Zuseher);
    }

    [TestMethod]
    public void Zuseherliste_einer_unbekannten_Praesentation_liefert_null()
    {
        _db.NeuePraesentation("Eins", 12, 8);

        Assert.IsNull(Dienst().Zuseherliste(99));
    }

    [TestMethod]
    public void ZuseherlisteCsv_hat_Kopfzeile_und_eine_Zeile_je_Zuseher()
    {
        var anna = _db.Schueler("Anna", "Muster");
        var klima = _db.NeuePraesentation("Klimawandel", 12, 8, "204");
        _db.Eintragen(anna, klima);

        var csv = Dienst().ZuseherlisteCsv(klima.Id);

        Assert.AreEqual(
            "Thema;Datum;Uhrzeit;Raum;Nachname;Vorname;E-Mail\r\n"
            + "Klimawandel;12.01.2027;08:00;204;Muster;Anna;anna.muster@stud.edubs.ch\r\n",
            csv);
    }

    [TestMethod]
    public void ZuseherlisteCsv_einer_unbekannten_Praesentation_liefert_null()
    {
        Assert.IsNull(Dienst().ZuseherlisteCsv(99));
    }

    [TestMethod]
    public void Die_CSV_schuetzt_Themen_die_wie_eine_Formel_beginnen()
    {
        _db.NeuePraesentation("=HYPERLINK(\"x\")", 12, 8, "204");

        var csv = Dienst().RaumuebersichtCsv();

        Assert.Contains("\"'=HYPERLINK(\"\"x\"\")\"", csv);
    }
}
