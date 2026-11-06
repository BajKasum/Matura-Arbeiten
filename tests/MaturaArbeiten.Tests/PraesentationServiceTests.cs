using MaturaArbeiten.Data;
using MaturaArbeiten.Services;

namespace MaturaArbeiten.Tests;

[TestClass]
public class PraesentationServiceTests
{
    private readonly AppDbContext _db = TestDb.Neu();

    private PraesentationService Dienst() => new(_db);

    [TestMethod]
    public void Liste_zeigt_bei_offener_Phase_alle_waehlbaren_Praesentationen_nach_Zeit()
    {
        var anna = _db.Schueler("Anna", "Muster");
        _db.NeuePraesentation("Spaet", 13, 8);
        _db.NeuePraesentation("Frueh", 12, 8);

        var ansicht = Dienst().Liste(anna.Id);

        CollectionAssert.AreEqual(new[] { "Frueh", "Spaet" }, ansicht.Zeilen.Select(z => z.Thema).ToArray());
        Assert.IsTrue(ansicht.PhaseOffen);
        Assert.AreEqual(0, ansicht.GewaehlteAnzahl);
        Assert.AreEqual(2, ansicht.Pflicht);
        Assert.IsTrue(ansicht.NochErreichbar);
        Assert.IsTrue(ansicht.Zeilen.All(z => z.FreiePlaetze == 6 && !z.Gewaehlt));
    }

    [TestMethod]
    public void Liste_markiert_die_eigene_Wahl_und_zaehlt_den_freien_Platz_ab()
    {
        var anna = _db.Schueler("Anna", "Muster");
        var klima = _db.NeuePraesentation("Klimawandel", 12, 8);
        _db.Eintragen(anna, klima);

        var zeile = Dienst().Liste(anna.Id).Zeilen.Single();

        Assert.IsTrue(zeile.Gewaehlt);
        Assert.AreEqual(5, zeile.FreiePlaetze);
    }

    [TestMethod]
    public void Liste_blendet_Praesentationen_zur_gleichen_Zeit_wie_die_eigene_Wahl_aus()
    {
        var anna = _db.Schueler("Anna", "Muster");
        var eigene = _db.NeuePraesentation("Eigene", 12, 8, "204");
        _db.NeuePraesentation("Gleiche Zeit", 12, 8, "302");
        _db.NeuePraesentation("Andere Zeit", 12, 9);
        _db.Eintragen(anna, eigene);

        var themen = Dienst().Liste(anna.Id).Zeilen.Select(z => z.Thema).ToArray();

        CollectionAssert.AreEqual(new[] { "Eigene", "Andere Zeit" }, themen);
    }

    [TestMethod]
    public void Liste_blendet_volle_Praesentationen_aus_ausser_man_hat_sie_selbst_gewaehlt()
    {
        _db.SetzeKontingent(1);
        var anna = _db.Schueler("Anna", "Muster");
        var ben = _db.Schueler("Ben", "Beispiel");
        var vollVonBen = _db.NeuePraesentation("Voll von Ben", 12, 8);
        var vollVonAnna = _db.NeuePraesentation("Voll von Anna", 12, 9);
        _db.NeuePraesentation("Frei", 12, 10);
        _db.Eintragen(ben, vollVonBen);
        _db.Eintragen(anna, vollVonAnna);

        var themen = Dienst().Liste(anna.Id).Zeilen.Select(z => z.Thema).ToArray();

        CollectionAssert.AreEqual(new[] { "Voll von Anna", "Frei" }, themen);
    }

    [TestMethod]
    public void Liste_zeigt_nach_zwei_Eintragungen_nur_noch_die_eigenen()
    {
        var anna = _db.Schueler("Anna", "Muster");
        var eins = _db.NeuePraesentation("Eins", 12, 8);
        var zwei = _db.NeuePraesentation("Zwei", 12, 9);
        _db.NeuePraesentation("Drei", 12, 10);
        _db.Eintragen(anna, eins);
        _db.Eintragen(anna, zwei);

        var ansicht = Dienst().Liste(anna.Id);

        CollectionAssert.AreEqual(new[] { "Eins", "Zwei" }, ansicht.Zeilen.Select(z => z.Thema).ToArray());
        Assert.AreEqual(2, ansicht.GewaehlteAnzahl);
        Assert.IsTrue(ansicht.NochErreichbar);
    }

    [TestMethod]
    public void Liste_zeigt_bei_geschlossener_Phase_nur_die_eigenen_Praesentationen()
    {
        var anna = _db.Schueler("Anna", "Muster");
        var eins = _db.NeuePraesentation("Eins", 12, 8);
        _db.NeuePraesentation("Zwei", 12, 9);
        _db.Eintragen(anna, eins);
        _db.SetzePhase(false);

        var ansicht = Dienst().Liste(anna.Id);

        Assert.IsFalse(ansicht.PhaseOffen);
        Assert.AreEqual("Eins", ansicht.Zeilen.Single().Thema);
    }

    [TestMethod]
    public void Liste_meldet_wenn_die_Pflicht_nicht_mehr_erreichbar_ist()
    {
        var anna = _db.Schueler("Anna", "Muster");
        _db.NeuePraesentation("Einzige", 12, 8);

        Assert.IsFalse(Dienst().Liste(anna.Id).NochErreichbar);
    }

    [TestMethod]
    public void Liste_ohne_jede_Praesentation_ist_leer()
    {
        var anna = _db.Schueler("Anna", "Muster");

        var ansicht = Dienst().Liste(anna.Id);

        Assert.IsEmpty(ansicht.Zeilen);
        Assert.IsFalse(ansicht.NochErreichbar);
    }

    [TestMethod]
    public void Alle_zeigt_Zuseher_und_freie_Plaetze_nach_Zeit_und_Raum_sortiert()
    {
        var anna = _db.Schueler("Anna", "Muster");
        var raum302 = _db.NeuePraesentation("B", 12, 8, "302");
        _db.NeuePraesentation("A", 12, 8, "204");
        _db.NeuePraesentation("C", 11, 8);
        _db.Eintragen(anna, raum302);

        var ansicht = Dienst().Alle();

        CollectionAssert.AreEqual(new[] { "C", "A", "B" }, ansicht.Zeilen.Select(z => z.Thema).ToArray());
        Assert.AreEqual(6, ansicht.Platzkontingent);
        var b = ansicht.Zeilen.Single(z => z.Thema == "B");
        Assert.AreEqual(1, b.Zuseher);
        Assert.AreEqual(5, b.FreiePlaetze);
    }

    [TestMethod]
    public void OhneMoeglichkeit_ist_leer_wenn_es_keine_Praesentationen_gibt()
    {
        _db.Schueler("Anna", "Muster");

        Assert.IsEmpty(Dienst().OhneMoeglichkeit());
    }

    [TestMethod]
    public void OhneMoeglichkeit_nennt_wer_die_Pflicht_nicht_mehr_erfuellen_kann()
    {
        var anna = _db.Schueler("Anna", "Muster");
        _db.Schueler("Ben", "Beispiel");
        var einzige = _db.NeuePraesentation("Einzige", 12, 8);
        _db.Eintragen(anna, einzige);

        var namen = Dienst().OhneMoeglichkeit();

        CollectionAssert.AreEqual(new[] { "Beispiel Ben", "Muster Anna" }, namen.ToArray());
    }

    [TestMethod]
    public void OhneMoeglichkeit_ist_leer_wenn_alle_zwei_Zeiten_erreichen_koennen()
    {
        _db.Schueler("Anna", "Muster");
        _db.NeuePraesentation("Eins", 12, 8);
        _db.NeuePraesentation("Zwei", 12, 9);

        Assert.IsEmpty(Dienst().OhneMoeglichkeit());
    }

    [TestMethod]
    public void OhneMoeglichkeit_erkennt_wenn_eine_volle_Praesentation_den_Weg_versperrt()
    {
        _db.SetzeKontingent(1);
        _db.Schueler("Anna", "Muster");
        var ben = _db.Schueler("Ben", "Beispiel");
        var eins = _db.NeuePraesentation("Eins", 12, 8);
        _db.NeuePraesentation("Zwei", 12, 9);
        _db.Eintragen(ben, eins);

        var namen = Dienst().OhneMoeglichkeit();

        CollectionAssert.AreEqual(new[] { "Muster Anna" }, namen.ToArray());
    }
}
