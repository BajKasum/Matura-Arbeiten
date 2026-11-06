using MaturaArbeiten.Data;
using MaturaArbeiten.Domain;
using MaturaArbeiten.Services;

namespace MaturaArbeiten.Tests;

[TestClass]
public class PasswortServiceTests
{
    private const string Alt = "EinLangesPasswort1";
    private const string Neu = "EinNeuesPasswort22";

    private readonly TestZeit _zeit = new();
    private readonly AppDbContext _db = TestDb.Neu();

    private PasswortService Dienst() =>
        new(_db, new AnmeldeService(_db, TestDb.Hasher, _zeit), TestDb.Hasher, _zeit);

    [TestMethod]
    public void G3SuS_aendert_das_Passwort_und_gilt_danach_als_geaendert()
    {
        var anna = _db.Schueler("Anna", "Muster", Alt);

        var fehler = Dienst().Aendern(Rollen.G3SuS, anna.Id, Alt, Neu);

        Assert.IsNull(fehler);
        Assert.IsTrue(_db.G3SuSListe.Single().PasswortGeaendert);
        var anmeldung = new AnmeldeService(_db, TestDb.Hasher, _zeit);
        Assert.IsInstanceOfType<AnmeldeErgebnis.Erfolg>(anmeldung.Anmelden(anna.Email, Neu));
        Assert.IsInstanceOfType<AnmeldeErgebnis.Abgewiesen>(anmeldung.Anmelden(anna.Email, Alt));
    }

    [TestMethod]
    public void Administration_aendert_das_Passwort()
    {
        var admin = _db.Admin("Ben", "Lehrer", Alt);

        var fehler = Dienst().Aendern(Rollen.Administration, admin.Id, Alt, Neu);

        Assert.IsNull(fehler);
        var anmeldung = new AnmeldeService(_db, TestDb.Hasher, _zeit);
        Assert.IsInstanceOfType<AnmeldeErgebnis.Erfolg>(anmeldung.Anmelden(admin.Email, Neu));
    }

    [TestMethod]
    public void Ein_falsches_bisheriges_Passwort_aendert_nichts()
    {
        var anna = _db.Schueler("Anna", "Muster", Alt);
        var hashVorher = anna.PasswortHash;

        var fehler = Dienst().Aendern(Rollen.G3SuS, anna.Id, "falsch", Neu);

        Assert.AreEqual("Das bisherige Passwort stimmt nicht.", fehler);
        Assert.AreEqual(hashVorher, _db.G3SuSListe.Single().PasswortHash);
        Assert.IsFalse(_db.G3SuSListe.Single().PasswortGeaendert);
    }

    [TestMethod]
    public void Ein_unbekanntes_Konto_wird_gemeldet()
    {
        Assert.AreEqual("Dieses Konto gibt es nicht mehr.", Dienst().Aendern(Rollen.G3SuS, 99, Alt, Neu));
        Assert.AreEqual("Dieses Konto gibt es nicht mehr.", Dienst().Aendern(Rollen.Administration, 99, Alt, Neu));
    }

    [TestMethod]
    public void Fuenf_falsche_bisherige_Passwoerter_sperren_das_Konto()
    {
        var anna = _db.Schueler("Anna", "Muster", Alt);
        var dienst = Dienst();
        for (var i = 0; i < 4; i++)
        {
            dienst.Aendern(Rollen.G3SuS, anna.Id, "falsch", Neu);
        }

        var fehler = dienst.Aendern(Rollen.G3SuS, anna.Id, "falsch", Neu);

        Assert.AreEqual("Zu viele falsche Versuche. Bitte warte noch 15 Minuten.", fehler);
        Assert.AreEqual(
            "Zu viele falsche Versuche. Bitte warte noch 15 Minuten.",
            dienst.Aendern(Rollen.G3SuS, anna.Id, Alt, Neu));
    }
}
