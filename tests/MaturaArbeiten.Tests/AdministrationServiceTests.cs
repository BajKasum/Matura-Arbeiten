using MaturaArbeiten.Data;
using MaturaArbeiten.Services;
using Microsoft.Extensions.Logging.Abstractions;
using MaturaArbeiten.ViewModels;

namespace MaturaArbeiten.Tests;

[TestClass]
public class AdministrationServiceTests
{
    private readonly AppDbContext _db = TestDb.Neu();

    private AdministrationService Dienst() => new(_db, new PraesentationService(_db), NullLogger<AdministrationService>.Instance);

    [TestMethod]
    public void PhaseUmschalten_wechselt_zwischen_offen_und_geschlossen()
    {
        var dienst = Dienst();

        Assert.IsFalse(dienst.PhaseUmschalten());
        Assert.IsFalse(_db.Systemeinstellungen.Single().EintragungsphaseOffen);
        Assert.IsTrue(dienst.PhaseUmschalten());
    }

    [TestMethod]
    public void Uebersicht_zaehlt_G3SuS_mit_erfuellter_Pflicht()
    {
        var anna = _db.Schueler("Anna", "Muster");
        _db.Schueler("Ben", "Beispiel");
        _db.Eintragen(anna, _db.NeuePraesentation("Eins", 12, 8));
        _db.Eintragen(anna, _db.NeuePraesentation("Zwei", 12, 9));

        var uebersicht = Dienst().Uebersicht();

        Assert.AreEqual(2, uebersicht.GesamtG3SuS);
        Assert.AreEqual(1, uebersicht.MitPflicht);
        Assert.AreEqual(2, uebersicht.Pflicht);
        Assert.IsTrue(uebersicht.PhaseOffen);
        Assert.IsFalse(uebersicht.AlleFertig);
    }

    [TestMethod]
    public void Uebersicht_ist_erst_fertig_wenn_alle_die_Pflicht_erfuellt_haben()
    {
        var anna = _db.Schueler("Anna", "Muster");
        _db.Eintragen(anna, _db.NeuePraesentation("Eins", 12, 8));
        _db.Eintragen(anna, _db.NeuePraesentation("Zwei", 12, 9));

        Assert.IsTrue(Dienst().Uebersicht().AlleFertig);
    }

    [TestMethod]
    public void Uebersicht_ohne_G3SuS_ist_nicht_fertig()
    {
        Assert.IsFalse(Dienst().Uebersicht().AlleFertig);
    }

    [TestMethod]
    public void Uebersicht_zaehlt_versendete_Mails_je_G3SuS_einmal_und_offene_Fehlschlaege()
    {
        var anna = _db.Schueler("Anna", "Muster");
        var ben = _db.Schueler("Ben", "Beispiel");
        var jetzt = DateTime.UtcNow;
        _db.EmailVersandListe.AddRange(
            new Models.EmailVersand { G3susId = anna.Id, Typ = "Zugangsdaten", Status = "Gesendet", GesendetAm = jetzt },
            new Models.EmailVersand { G3susId = anna.Id, Typ = "Zugangsdaten", Status = "Gesendet", GesendetAm = jetzt.AddMinutes(1) },
            new Models.EmailVersand { G3susId = ben.Id, Typ = "Zugangsdaten", Status = "Fehlgeschlagen", GesendetAm = jetzt },
            new Models.EmailVersand { G3susId = anna.Id, Typ = "Bestaetigung", Status = "Gesendet", GesendetAm = jetzt });
        _db.SaveChanges();

        var uebersicht = Dienst().Uebersicht();

        Assert.AreEqual(1, uebersicht.ZugangsdatenVersendet);
        Assert.AreEqual(1, uebersicht.BestaetigungenVersendet);
        Assert.AreEqual(1, uebersicht.Fehlgeschlagen);
    }

    [TestMethod]
    public void Vorschau_zaehlt_alles_was_beim_Zuruecksetzen_geloescht_wird()
    {
        var anna = _db.Schueler("Anna", "Muster");
        _db.Eintragen(anna, _db.NeuePraesentation("Eins", 12, 8));

        var vorschau = Dienst().Vorschau();

        Assert.AreEqual(new ZuruecksetzenViewModel(1, 1, 1, 0), vorschau);
    }

    [TestMethod]
    public void Zuruecksetzen_loescht_Daten_aber_behaelt_Administrationslogins_und_Kontingent()
    {
        var anna = _db.Schueler("Anna", "Muster");
        _db.Admin("Ben", "Lehrer");
        _db.SetzeKontingent(4);
        _db.SetzePhase(false);
        _db.Eintragen(anna, _db.NeuePraesentation("Eins", 12, 8));
        _db.EmailVersandListe.Add(new Models.EmailVersand
        {
            G3susId = anna.Id,
            Typ = "Zugangsdaten",
            Status = "Gesendet",
            GesendetAm = DateTime.UtcNow
        });
        _db.SaveChanges();

        Dienst().Zuruecksetzen();

        Assert.AreEqual(0, _db.G3SuSListe.Count());
        Assert.AreEqual(0, _db.Praesentationen.Count());
        Assert.AreEqual(0, _db.Eintragungen.Count());
        Assert.AreEqual(0, _db.EmailVersandListe.Count());
        Assert.AreEqual(1, _db.Administrationslogins.Count());
        Assert.AreEqual(4, _db.Systemeinstellungen.Single().Platzkontingent);
        Assert.IsTrue(_db.Systemeinstellungen.Single().EintragungsphaseOffen);
    }
}
