using MaturaArbeiten.Data;
using MaturaArbeiten.Domain;
using MaturaArbeiten.Services;

namespace MaturaArbeiten.Tests;

[TestClass]
public class EintragungServiceTests
{
    private readonly TestZeit _zeit = new();
    private readonly AppDbContext _db = TestDb.Neu();

    private EintragungService Dienst() => new(_db, _zeit);

    private static EintragungErgebnis Abgewiesen(Abweisung grund) => new EintragungErgebnis.Abgewiesen(grund);

    [TestMethod]
    public void Eintragen_speichert_die_Eintragung_mit_der_aktuellen_Zeit()
    {
        var anna = _db.Schueler("Anna", "Muster");
        var klima = _db.NeuePraesentation("Klimawandel", 12, 8);

        var ergebnis = Dienst().Eintragen(anna.Id, klima.Id);

        Assert.AreEqual(new EintragungErgebnis.Erfolg(), ergebnis);
        var eintragung = _db.Eintragungen.Single();
        Assert.AreEqual(anna.Id, eintragung.G3susId);
        Assert.AreEqual(klima.Id, eintragung.PraesentationId);
        Assert.AreEqual(_zeit.GetUtcNow().UtcDateTime, eintragung.ErstelltAm);
    }

    [TestMethod]
    public void Eintragen_bei_geschlossener_Phase_wird_abgewiesen_und_nichts_gespeichert()
    {
        var anna = _db.Schueler("Anna", "Muster");
        var klima = _db.NeuePraesentation("Klimawandel", 12, 8);
        _db.SetzePhase(false);

        var ergebnis = Dienst().Eintragen(anna.Id, klima.Id);

        Assert.AreEqual(Abgewiesen(Abweisung.PhaseGeschlossen), ergebnis);
        Assert.AreEqual(0, _db.Eintragungen.Count());
    }

    [TestMethod]
    public void Die_dritte_Eintragung_wird_abgewiesen()
    {
        var anna = _db.Schueler("Anna", "Muster");
        var eins = _db.NeuePraesentation("Eins", 12, 8);
        var zwei = _db.NeuePraesentation("Zwei", 12, 9);
        var drei = _db.NeuePraesentation("Drei", 12, 10);
        var dienst = Dienst();
        dienst.Eintragen(anna.Id, eins.Id);
        dienst.Eintragen(anna.Id, zwei.Id);

        var ergebnis = dienst.Eintragen(anna.Id, drei.Id);

        Assert.AreEqual(Abgewiesen(Abweisung.SchonZweiEintragungen), ergebnis);
        Assert.AreEqual(2, _db.Eintragungen.Count());
    }

    [TestMethod]
    public void Eine_volle_Praesentation_nimmt_niemanden_mehr_auf()
    {
        _db.SetzeKontingent(1);
        var anna = _db.Schueler("Anna", "Muster");
        var ben = _db.Schueler("Ben", "Beispiel");
        var klima = _db.NeuePraesentation("Klimawandel", 12, 8);
        var dienst = Dienst();
        dienst.Eintragen(anna.Id, klima.Id);

        var ergebnis = dienst.Eintragen(ben.Id, klima.Id);

        Assert.AreEqual(Abgewiesen(Abweisung.PraesentationVoll), ergebnis);
    }

    [TestMethod]
    public void Zwei_Praesentationen_zur_gleichen_Zeit_sind_eine_Terminkollision()
    {
        var anna = _db.Schueler("Anna", "Muster");
        var raum204 = _db.NeuePraesentation("Eins", 12, 8, "204");
        var raum302 = _db.NeuePraesentation("Zwei", 12, 8, "302");
        var dienst = Dienst();
        dienst.Eintragen(anna.Id, raum204.Id);

        var ergebnis = dienst.Eintragen(anna.Id, raum302.Id);

        Assert.AreEqual(Abgewiesen(Abweisung.Terminkollision), ergebnis);
    }

    [TestMethod]
    public void Dieselbe_Praesentation_kann_nicht_zweimal_gewaehlt_werden()
    {
        var anna = _db.Schueler("Anna", "Muster");
        var klima = _db.NeuePraesentation("Klimawandel", 12, 8);
        var dienst = Dienst();
        dienst.Eintragen(anna.Id, klima.Id);

        Assert.AreEqual(Abgewiesen(Abweisung.Terminkollision), dienst.Eintragen(anna.Id, klima.Id));
        Assert.AreEqual(1, _db.Eintragungen.Count());
    }

    [TestMethod]
    public void Die_Administration_darf_auch_bei_geschlossener_Phase_eintragen()
    {
        var anna = _db.Schueler("Anna", "Muster");
        var klima = _db.NeuePraesentation("Klimawandel", 12, 8);
        _db.SetzePhase(false);

        var ergebnis = Dienst().EintragenDurchAdministration(anna.Id, klima.Id);

        Assert.AreEqual(new EintragungErgebnis.Erfolg(), ergebnis);
    }

    [TestMethod]
    public void Die_Administration_kann_trotzdem_keine_volle_Praesentation_ueberbuchen()
    {
        _db.SetzeKontingent(1);
        var anna = _db.Schueler("Anna", "Muster");
        var ben = _db.Schueler("Ben", "Beispiel");
        var klima = _db.NeuePraesentation("Klimawandel", 12, 8);
        _db.Eintragen(anna, klima);

        var ergebnis = Dienst().EintragenDurchAdministration(ben.Id, klima.Id);

        Assert.AreEqual(Abgewiesen(Abweisung.PraesentationVoll), ergebnis);
    }

    [TestMethod]
    public void Aendern_verschiebt_die_Eintragung_auf_eine_andere_Praesentation()
    {
        var anna = _db.Schueler("Anna", "Muster");
        var eins = _db.NeuePraesentation("Eins", 12, 8);
        var zwei = _db.NeuePraesentation("Zwei", 12, 9);
        var eintragung = _db.Eintragen(anna, eins);

        var ergebnis = Dienst().Aendern(eintragung.Id, zwei.Id);

        Assert.AreEqual(new EintragungErgebnis.Erfolg(), ergebnis);
        Assert.AreEqual(zwei.Id, _db.Eintragungen.Single().PraesentationId);
    }

    [TestMethod]
    public void Aendern_einer_unbekannten_Eintragung_liefert_null()
    {
        var klima = _db.NeuePraesentation("Klimawandel", 12, 8);

        Assert.IsNull(Dienst().Aendern(99, klima.Id));
    }

    [TestMethod]
    public void Aendern_zaehlt_die_geaenderte_Eintragung_nicht_als_dritte()
    {
        var anna = _db.Schueler("Anna", "Muster");
        var eins = _db.NeuePraesentation("Eins", 12, 8);
        var zwei = _db.NeuePraesentation("Zwei", 12, 9);
        var drei = _db.NeuePraesentation("Drei", 12, 10);
        _db.Eintragen(anna, eins);
        var zweite = _db.Eintragen(anna, zwei);

        var ergebnis = Dienst().Aendern(zweite.Id, drei.Id);

        Assert.AreEqual(new EintragungErgebnis.Erfolg(), ergebnis);
    }

    [TestMethod]
    public void Aendern_auf_dieselbe_Praesentation_ist_erlaubt()
    {
        var anna = _db.Schueler("Anna", "Muster");
        var eins = _db.NeuePraesentation("Eins", 12, 8);
        var eintragung = _db.Eintragen(anna, eins);

        Assert.AreEqual(new EintragungErgebnis.Erfolg(), Dienst().Aendern(eintragung.Id, eins.Id));
    }

    [TestMethod]
    public void Aendern_in_eine_volle_Praesentation_wird_abgewiesen_und_laesst_alles_unveraendert()
    {
        _db.SetzeKontingent(1);
        var anna = _db.Schueler("Anna", "Muster");
        var ben = _db.Schueler("Ben", "Beispiel");
        var eins = _db.NeuePraesentation("Eins", 12, 8);
        var zwei = _db.NeuePraesentation("Zwei", 12, 9);
        var eintragung = _db.Eintragen(anna, eins);
        _db.Eintragen(ben, zwei);

        var ergebnis = Dienst().Aendern(eintragung.Id, zwei.Id);

        Assert.AreEqual(Abgewiesen(Abweisung.PraesentationVoll), ergebnis);
        Assert.AreEqual(eins.Id, _db.Eintragungen.Single(e => e.Id == eintragung.Id).PraesentationId);
    }

    [TestMethod]
    public void Aendern_auf_die_Zeit_der_anderen_Eintragung_ist_eine_Terminkollision()
    {
        var anna = _db.Schueler("Anna", "Muster");
        var eins = _db.NeuePraesentation("Eins", 12, 8);
        var zwei = _db.NeuePraesentation("Zwei", 12, 9);
        var dritteZurZeitVonZwei = _db.NeuePraesentation("Drei", 12, 9, "302");
        var erste = _db.Eintragen(anna, eins);
        _db.Eintragen(anna, zwei);

        var ergebnis = Dienst().Aendern(erste.Id, dritteZurZeitVonZwei.Id);

        Assert.AreEqual(Abgewiesen(Abweisung.Terminkollision), ergebnis);
    }

    [TestMethod]
    public void Aendern_ist_auch_bei_geschlossener_Phase_moeglich()
    {
        var anna = _db.Schueler("Anna", "Muster");
        var eins = _db.NeuePraesentation("Eins", 12, 8);
        var zwei = _db.NeuePraesentation("Zwei", 12, 9);
        var eintragung = _db.Eintragen(anna, eins);
        _db.SetzePhase(false);

        Assert.AreEqual(new EintragungErgebnis.Erfolg(), Dienst().Aendern(eintragung.Id, zwei.Id));
    }

    [TestMethod]
    public void Loeschen_entfernt_die_Eintragung_und_meldet_ob_es_sie_gab()
    {
        var anna = _db.Schueler("Anna", "Muster");
        var eintragung = _db.Eintragen(anna, _db.NeuePraesentation("Eins", 12, 8));
        var dienst = Dienst();

        Assert.IsTrue(dienst.Loeschen(eintragung.Id));
        Assert.AreEqual(0, _db.Eintragungen.Count());
        Assert.IsFalse(dienst.Loeschen(eintragung.Id));
    }

    [TestMethod]
    public void Meine_zeigt_nur_die_eigenen_Eintragungen_nach_Zeit_sortiert()
    {
        var anna = _db.Schueler("Anna", "Muster");
        var ben = _db.Schueler("Ben", "Beispiel");
        var spaet = _db.NeuePraesentation("Spaet", 13, 8);
        var frueh = _db.NeuePraesentation("Frueh", 12, 8);
        var fremd = _db.NeuePraesentation("Fremd", 12, 9);
        _db.Eintragen(anna, spaet);
        _db.Eintragen(anna, frueh);
        _db.Eintragen(ben, fremd);

        var ansicht = Dienst().Meine(anna.Id);

        CollectionAssert.AreEqual(new[] { "Frueh", "Spaet" }, ansicht.Zeilen.Select(z => z.Thema).ToArray());
        Assert.AreEqual(2, ansicht.Pflicht);
        Assert.IsTrue(ansicht.PflichtErfuellt);
    }

    [TestMethod]
    public void Meine_mit_einer_Eintragung_erfuellt_die_Pflicht_noch_nicht()
    {
        var anna = _db.Schueler("Anna", "Muster");
        _db.Eintragen(anna, _db.NeuePraesentation("Eins", 12, 8));

        Assert.IsFalse(Dienst().Meine(anna.Id).PflichtErfuellt);
    }

    [TestMethod]
    public void Alle_sortiert_nach_Nachname_Vorname_und_Zeit()
    {
        var muster = _db.Schueler("Anna", "Muster");
        var beispiel = _db.Schueler("Ben", "Beispiel");
        var frueh = _db.NeuePraesentation("Frueh", 12, 8);
        var spaet = _db.NeuePraesentation("Spaet", 12, 9);
        _db.Eintragen(muster, spaet);
        _db.Eintragen(muster, frueh);
        _db.Eintragen(beispiel, spaet);

        var zeilen = Dienst().Alle().Zeilen;

        CollectionAssert.AreEqual(
            new[] { "Beispiel Ben|Spaet", "Muster Anna|Frueh", "Muster Anna|Spaet" },
            zeilen.Select(z => $"{z.G3SuS}|{z.Thema}").ToArray());
    }

    [TestMethod]
    public void Formular_ohne_Id_listet_G3SuS_und_Praesentationen_mit_Stand()
    {
        var anna = _db.Schueler("Anna", "Muster");
        var klima = _db.NeuePraesentation("Klimawandel", 12, 8);
        _db.Eintragen(anna, klima);

        var formular = Dienst().Formular(null)!;

        Assert.IsNull(formular.G3SuSName);
        Assert.AreEqual("Muster Anna (1 von 2)", formular.G3SuS.Single().Text);
        Assert.AreEqual("Klimawandel, 12.01.2027 08:00, Raum 204 (5 frei)", formular.Praesentationen.Single().Text);
    }

    [TestMethod]
    public void Formular_mit_Id_enthaelt_die_vorhandene_Eintragung_und_den_Namen()
    {
        var anna = _db.Schueler("Anna", "Muster");
        var klima = _db.NeuePraesentation("Klimawandel", 12, 8);
        var eintragung = _db.Eintragen(anna, klima);

        var formular = Dienst().Formular(eintragung.Id)!;

        Assert.AreEqual("Muster Anna", formular.G3SuSName);
        Assert.AreEqual(anna.Id, formular.Eingabe.G3susId);
        Assert.AreEqual(klima.Id, formular.Eingabe.PraesentationId);
    }

    [TestMethod]
    public void Formular_mit_unbekannter_Id_liefert_null()
    {
        Assert.IsNull(Dienst().Formular(99));
    }
}
