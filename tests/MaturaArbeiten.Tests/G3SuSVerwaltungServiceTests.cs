using MaturaArbeiten.Data;
using MaturaArbeiten.Services;
using MaturaArbeiten.ViewModels;

namespace MaturaArbeiten.Tests;

[TestClass]
public class G3SuSVerwaltungServiceTests
{
    private readonly AppDbContext _db = TestDb.Neu();

    private G3SuSVerwaltungService Dienst() => new(_db);

    private static PersonFormular Eingabe(string vorname, string nachname, string email) =>
        new() { Vorname = vorname, Nachname = nachname, Email = email };

    [TestMethod]
    public void Anlegen_speichert_getrimmt()
    {
        var fehler = Dienst().Anlegen(Eingabe(" Anna ", " Muster ", " anna@x.ch "));

        Assert.IsNull(fehler);
        var g = _db.G3SuSListe.Single();
        Assert.AreEqual("Anna", g.Vorname);
        Assert.AreEqual("Muster", g.Nachname);
        Assert.AreEqual("anna@x.ch", g.Email);
    }

    [TestMethod]
    public void Anlegen_mit_vergebener_Adresse_wird_abgewiesen_auch_bei_anderer_Schreibweise()
    {
        _db.Schueler("Anna", "Muster");

        var fehler = Dienst().Anlegen(Eingabe("Neu", "Neu", "ANNA.MUSTER@stud.edubs.ch"));

        Assert.AreEqual("Diese E-Mail-Adresse gibt es schon.", fehler);
        Assert.AreEqual(1, _db.G3SuSListe.Count());
    }

    [TestMethod]
    public void Anlegen_mit_der_Adresse_eines_Administrationslogins_wird_abgewiesen()
    {
        var admin = _db.Admin("Ben", "Lehrer");

        Assert.AreEqual(
            "Diese E-Mail-Adresse gibt es schon.",
            Dienst().Anlegen(Eingabe("Neu", "Neu", admin.Email)));
    }

    [TestMethod]
    public void Aendern_uebernimmt_die_Werte_und_erlaubt_die_eigene_Adresse_weiter()
    {
        var anna = _db.Schueler("Anna", "Muster");

        var fehler = Dienst().Aendern(anna.Id, Eingabe("Anne", "Muster", anna.Email));

        Assert.IsNull(fehler);
        Assert.AreEqual("Anne", _db.G3SuSListe.Single().Vorname);
    }

    [TestMethod]
    public void Aendern_auf_die_Adresse_eines_anderen_wird_abgewiesen()
    {
        var anna = _db.Schueler("Anna", "Muster");
        var ben = _db.Schueler("Ben", "Beispiel");

        Assert.AreEqual(
            "Diese E-Mail-Adresse gibt es schon.",
            Dienst().Aendern(anna.Id, Eingabe("Anna", "Muster", ben.Email)));
    }

    [TestMethod]
    public void Aendern_eines_unbekannten_G3SuS_wird_gemeldet()
    {
        Assert.AreEqual("Diesen G3-SuS gibt es nicht mehr.", Dienst().Aendern(99, Eingabe("A", "B", "a@b.ch")));
    }

    [TestMethod]
    public void Finden_liefert_das_Formular_oder_null()
    {
        var anna = _db.Schueler("Anna", "Muster");

        var formular = Dienst().Finden(anna.Id)!;

        Assert.AreEqual("Anna", formular.Vorname);
        Assert.AreEqual(anna.Email, formular.Email);
        Assert.IsNull(Dienst().Finden(99));
    }

    [TestMethod]
    public void Liste_zeigt_Anzahl_der_Eintragungen_und_ob_ein_Passwort_vergeben_ist()
    {
        var anna = _db.Schueler("Anna", "Muster", "EinLangesPasswort1");
        _db.Schueler("Ben", "Beispiel");
        _db.Eintragen(anna, _db.NeuePraesentation("Eins", 12, 8));

        var zeilen = Dienst().Liste();

        Assert.AreEqual("Beispiel", zeilen[0].Nachname);
        Assert.AreEqual(0, zeilen[0].Eintragungen);
        Assert.IsFalse(zeilen[0].HatPasswort);
        Assert.AreEqual(1, zeilen[1].Eintragungen);
        Assert.IsTrue(zeilen[1].HatPasswort);
    }

    [TestMethod]
    public void Loeschen_entfernt_den_G3SuS_samt_Eintragungen_und_Mailprotokoll()
    {
        var anna = _db.Schueler("Anna", "Muster");
        var ben = _db.Schueler("Ben", "Beispiel");
        var klima = _db.NeuePraesentation("Klimawandel", 12, 8);
        _db.Eintragen(anna, klima);
        _db.Eintragen(ben, klima);
        _db.EmailVersandListe.Add(new Models.EmailVersand
        {
            G3susId = anna.Id,
            Typ = "Zugangsdaten",
            Status = "Gesendet",
            GesendetAm = DateTime.UtcNow
        });
        _db.SaveChanges();

        var geloescht = Dienst().Loeschen(anna.Id);

        Assert.IsTrue(geloescht);
        Assert.AreEqual(1, _db.G3SuSListe.Count());
        Assert.AreEqual(ben.Id, _db.Eintragungen.Single().G3susId);
        Assert.AreEqual(0, _db.EmailVersandListe.Count());
        Assert.IsFalse(Dienst().Loeschen(anna.Id));
    }
}
