using MaturaArbeiten.Data;
using MaturaArbeiten.Services;
using MaturaArbeiten.ViewModels;

namespace MaturaArbeiten.Tests;

[TestClass]
public class AdminLoginServiceTests
{
    private readonly AppDbContext _db = TestDb.Neu();

    private AdminLoginService Dienst() => new(_db);

    private static PersonFormular Eingabe(string vorname, string nachname, string email) =>
        new() { Vorname = vorname, Nachname = nachname, Email = email };

    [TestMethod]
    public void Anlegen_liefert_die_neue_Id()
    {
        var (id, fehler) = Dienst().Anlegen(Eingabe("Ben", "Lehrer", "ben@schule.ch"));

        Assert.IsNull(fehler);
        Assert.AreEqual(_db.Administrationslogins.Single().Id, id);
    }

    [TestMethod]
    public void Anlegen_mit_der_Adresse_eines_G3SuS_wird_abgewiesen()
    {
        var anna = _db.Schueler("Anna", "Muster");

        var (id, fehler) = Dienst().Anlegen(Eingabe("Ben", "Lehrer", anna.Email.ToUpperInvariant()));

        Assert.IsNull(id);
        Assert.AreEqual("Diese E-Mail-Adresse gibt es schon.", fehler);
    }

    [TestMethod]
    public void Aendern_und_Finden_arbeiten_zusammen()
    {
        var admin = _db.Admin("Ben", "Lehrer");

        var fehler = Dienst().Aendern(admin.Id, Eingabe("Benno", "Lehrer", admin.Email));

        Assert.IsNull(fehler);
        Assert.AreEqual("Benno", Dienst().Finden(admin.Id)!.Vorname);
        Assert.IsNull(Dienst().Finden(99));
        Assert.AreEqual(
            "Dieses Administrationslogin gibt es nicht mehr.",
            Dienst().Aendern(99, Eingabe("A", "B", "a@b.ch")));
    }

    [TestMethod]
    public void Das_letzte_Login_kann_nicht_geloescht_werden()
    {
        var admin = _db.Admin("Ben", "Lehrer");

        var fehler = Dienst().Loeschen(admin.Id);

        Assert.AreEqual("Das letzte Administrationslogin kann nicht gelöscht werden.", fehler);
        Assert.AreEqual(1, _db.Administrationslogins.Count());
    }

    [TestMethod]
    public void Ein_Login_unter_mehreren_kann_geloescht_werden()
    {
        var ben = _db.Admin("Ben", "Lehrer");
        _db.Admin("Clara", "Lehrerin");

        Assert.IsNull(Dienst().Loeschen(ben.Id));
        Assert.AreEqual(1, _db.Administrationslogins.Count());
        Assert.AreEqual(
            "Dieses Administrationslogin gibt es nicht mehr.",
            Dienst().Loeschen(ben.Id));
    }

    [TestMethod]
    public void Liste_zeigt_ob_ein_Passwort_vergeben_ist()
    {
        _db.Admin("Ben", "Lehrer", "EinLangesPasswort1");
        _db.Admin("Clara", "Anders");

        var zeilen = Dienst().Liste();

        Assert.AreEqual("Anders", zeilen[0].Nachname);
        Assert.IsFalse(zeilen[0].HatPasswort);
        Assert.IsTrue(zeilen[1].HatPasswort);
    }
}
