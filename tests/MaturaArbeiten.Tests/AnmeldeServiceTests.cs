using MaturaArbeiten.Data;
using MaturaArbeiten.Domain;
using MaturaArbeiten.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace MaturaArbeiten.Tests;

[TestClass]
public class AnmeldeServiceTests
{
    private const string Passwort = "EinLangesPasswort1";

    private readonly TestZeit _zeit = new();
    private readonly AppDbContext _db = TestDb.Neu();

    private AnmeldeService Dienst() => new(_db, TestDb.Hasher, _zeit, NullLogger<AnmeldeService>.Instance);

    [TestMethod]
    public void Eine_unbekannte_Adresse_wird_abgewiesen()
    {
        Assert.AreEqual(new AnmeldeErgebnis.Abgewiesen(), Dienst().Anmelden("niemand@x.ch", Passwort));
    }

    [TestMethod]
    public void G3SuS_mit_richtigem_Passwort_meldet_sich_an_und_muss_das_Passwort_aendern()
    {
        var anna = _db.Schueler("Anna", "Muster", Passwort);

        var ergebnis = Dienst().Anmelden(anna.Email, Passwort);

        Assert.AreEqual(new AnmeldeErgebnis.Erfolg(anna.Id, "Anna Muster", Rollen.G3SuS, true), ergebnis);
    }

    [TestMethod]
    public void G3SuS_nach_der_Passwortaenderung_muss_nichts_mehr_aendern()
    {
        var anna = _db.Schueler("Anna", "Muster", Passwort);
        anna.PasswortGeaendert = true;
        _db.SaveChanges();

        var ergebnis = Dienst().Anmelden(anna.Email, Passwort);

        Assert.AreEqual(new AnmeldeErgebnis.Erfolg(anna.Id, "Anna Muster", Rollen.G3SuS, false), ergebnis);
    }

    [TestMethod]
    public void Administration_meldet_sich_mit_der_Rolle_Administration_an()
    {
        var admin = _db.Admin("Ben", "Lehrer", Passwort);

        var ergebnis = Dienst().Anmelden(admin.Email, Passwort);

        Assert.AreEqual(new AnmeldeErgebnis.Erfolg(admin.Id, "Ben Lehrer", Rollen.Administration, false), ergebnis);
    }

    [TestMethod]
    public void Ein_falsches_Passwort_wird_abgewiesen_und_gezaehlt()
    {
        var anna = _db.Schueler("Anna", "Muster", Passwort);

        var ergebnis = Dienst().Anmelden(anna.Email, "falsch");

        Assert.AreEqual(new AnmeldeErgebnis.Abgewiesen(), ergebnis);
        Assert.AreEqual(1, _db.G3SuSListe.Single().Fehlversuche);
    }

    [TestMethod]
    public void Ein_Konto_ohne_Passwort_kann_sich_nicht_anmelden()
    {
        var anna = _db.Schueler("Anna", "Muster");

        Assert.AreEqual(new AnmeldeErgebnis.Abgewiesen(), Dienst().Anmelden(anna.Email, "irgendwas"));
        Assert.AreEqual(0, _db.G3SuSListe.Single().Fehlversuche);
    }

    [TestMethod]
    public void Ein_richtiges_Passwort_setzt_die_Fehlversuche_zurueck()
    {
        var anna = _db.Schueler("Anna", "Muster", Passwort);
        var dienst = Dienst();

        dienst.Anmelden(anna.Email, "falsch");
        dienst.Anmelden(anna.Email, "falsch");
        dienst.Anmelden(anna.Email, Passwort);

        Assert.AreEqual(0, _db.G3SuSListe.Single().Fehlversuche);
    }

    [TestMethod]
    public void Das_fuenfte_falsche_Passwort_sperrt_das_Konto_fuer_fuenfzehn_Minuten()
    {
        var anna = _db.Schueler("Anna", "Muster", Passwort);
        var dienst = Dienst();

        for (var i = 0; i < 4; i++)
        {
            Assert.AreEqual(new AnmeldeErgebnis.Abgewiesen(), dienst.Anmelden(anna.Email, "falsch"));
        }

        var ergebnis = dienst.Anmelden(anna.Email, "falsch");

        Assert.AreEqual(new AnmeldeErgebnis.Gesperrt(_zeit.GetUtcNow().UtcDateTime.AddMinutes(15)), ergebnis);
    }

    [TestMethod]
    public void Waehrend_der_Sperre_wird_auch_das_richtige_Passwort_nicht_angenommen()
    {
        var anna = _db.Schueler("Anna", "Muster", Passwort);
        var dienst = Dienst();
        for (var i = 0; i < 5; i++)
        {
            dienst.Anmelden(anna.Email, "falsch");
        }

        _zeit.Vor(TimeSpan.FromMinutes(14));

        Assert.IsInstanceOfType<AnmeldeErgebnis.Gesperrt>(dienst.Anmelden(anna.Email, Passwort));
    }

    [TestMethod]
    public void Nach_der_Sperre_geht_die_Anmeldung_wieder()
    {
        var anna = _db.Schueler("Anna", "Muster", Passwort);
        var dienst = Dienst();
        for (var i = 0; i < 5; i++)
        {
            dienst.Anmelden(anna.Email, "falsch");
        }

        _zeit.Vor(TimeSpan.FromMinutes(16));

        Assert.IsInstanceOfType<AnmeldeErgebnis.Erfolg>(dienst.Anmelden(anna.Email, Passwort));
        Assert.IsNull(_db.G3SuSListe.Single().GesperrtBis);
    }

    [TestMethod]
    public void Auch_ein_Administrationslogin_wird_nach_fuenf_Fehlversuchen_gesperrt()
    {
        var admin = _db.Admin("Ben", "Lehrer", Passwort);
        var dienst = Dienst();
        for (var i = 0; i < 4; i++)
        {
            dienst.Anmelden(admin.Email, "falsch");
        }

        Assert.IsInstanceOfType<AnmeldeErgebnis.Gesperrt>(dienst.Anmelden(admin.Email, "falsch"));
        Assert.IsInstanceOfType<AnmeldeErgebnis.Gesperrt>(dienst.Anmelden(admin.Email, Passwort));
    }

    [TestMethod]
    public void Die_Sperre_eines_Kontos_betrifft_andere_Konten_nicht()
    {
        var anna = _db.Schueler("Anna", "Muster", Passwort);
        var ben = _db.Schueler("Ben", "Beispiel", Passwort);
        var dienst = Dienst();
        for (var i = 0; i < 5; i++)
        {
            dienst.Anmelden(anna.Email, "falsch");
        }

        Assert.IsInstanceOfType<AnmeldeErgebnis.Erfolg>(dienst.Anmelden(ben.Email, Passwort));
    }
}
