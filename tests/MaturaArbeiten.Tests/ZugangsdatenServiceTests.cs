using MaturaArbeiten.Data;
using MaturaArbeiten.Domain;
using MaturaArbeiten.Services;
using Microsoft.AspNetCore.Identity;

namespace MaturaArbeiten.Tests;

[TestClass]
public class ZugangsdatenServiceTests
{
    private readonly TestZeit _zeit = new();
    private readonly TestEmail _email = new();
    private readonly AppDbContext _db = TestDb.Neu();

    private ZugangsdatenService Dienst() => new(_db, _email, TestDb.Hasher, _zeit);

    private static string PasswortAus(Mail mail) =>
        mail.Text.Split('\n').Single(z => z.StartsWith("Passwort: ")).Substring("Passwort: ".Length).Trim();

    [TestMethod]
    public void Versenden_schickt_nur_an_G3SuS_ohne_Passwort_und_protokolliert()
    {
        var anna = _db.Schueler("Anna", "Muster");
        _db.Schueler("Ben", "Beispiel", "SchonVergeben123");

        var (gesendet, fehlgeschlagen) = Dienst().Versenden();

        Assert.AreEqual(1, gesendet);
        Assert.AreEqual(0, fehlgeschlagen);
        Assert.AreEqual(anna.Email, _email.Gesendet.Single().An);
        var eintrag = _db.EmailVersandListe.Single();
        Assert.AreEqual(anna.Id, eintrag.G3susId);
        Assert.AreEqual("Zugangsdaten", eintrag.Typ);
        Assert.AreEqual("Gesendet", eintrag.Status);
        Assert.AreEqual(_zeit.GetUtcNow().UtcDateTime, eintrag.GesendetAm);
    }

    [TestMethod]
    public void Das_gemailte_Passwort_passt_zum_gespeicherten_Hash_und_ist_zwoelf_Zeichen_lang()
    {
        var anna = _db.Schueler("Anna", "Muster");

        Dienst().Versenden();

        var passwort = PasswortAus(_email.Gesendet.Single());
        Assert.AreEqual(12, passwort.Length);
        var hash = _db.G3SuSListe.Single().PasswortHash!;
        Assert.AreNotEqual(
            PasswordVerificationResult.Failed,
            TestDb.Hasher.VerifyHashedPassword(anna.Email, hash, passwort));
        Assert.IsFalse(_db.G3SuSListe.Single().PasswortGeaendert);
    }

    [TestMethod]
    public void Jeder_G3SuS_bekommt_ein_eigenes_Passwort()
    {
        _db.Schueler("Anna", "Muster");
        _db.Schueler("Ben", "Beispiel");

        Dienst().Versenden();

        Assert.AreEqual(2, _email.Gesendet.Select(PasswortAus).Distinct().Count());
    }

    [TestMethod]
    public void Bei_Fehlschlag_wird_kein_Passwort_gespeichert_aber_der_Fehler_protokolliert()
    {
        _db.Schueler("Anna", "Muster");
        _email.Erfolg = false;

        var (gesendet, fehlgeschlagen) = Dienst().Versenden();

        Assert.AreEqual(0, gesendet);
        Assert.AreEqual(1, fehlgeschlagen);
        Assert.IsNull(_db.G3SuSListe.Single().PasswortHash);
        Assert.AreEqual("Fehlgeschlagen", _db.EmailVersandListe.Single().Status);
    }

    [TestMethod]
    public void Ein_zweiter_Versand_erreicht_nur_noch_die_Fehlgeschlagenen()
    {
        _db.Schueler("Anna", "Muster");
        _email.Erfolg = false;
        Dienst().Versenden();
        _email.Erfolg = true;

        var (gesendet, _) = Dienst().Versenden();
        var (nochmals, _) = Dienst().Versenden();

        Assert.AreEqual(1, gesendet);
        Assert.AreEqual(0, nochmals);
    }

    [TestMethod]
    public void AnG3SuS_setzt_das_Passwort_neu_und_hebt_Sperre_und_Aenderungsstatus_auf()
    {
        var anna = _db.Schueler("Anna", "Muster", "AltesPasswort123");
        anna.PasswortGeaendert = true;
        anna.Fehlversuche = 3;
        anna.GesperrtBis = DateTime.UtcNow.AddMinutes(10);
        _db.SaveChanges();

        var erfolg = Dienst().AnG3SuS(anna.Id);

        Assert.IsTrue(erfolg);
        var neu = _db.G3SuSListe.Single();
        Assert.IsFalse(neu.PasswortGeaendert);
        Assert.AreEqual(0, neu.Fehlversuche);
        Assert.IsNull(neu.GesperrtBis);
        Assert.AreEqual(
            PasswordVerificationResult.Failed,
            TestDb.Hasher.VerifyHashedPassword(anna.Email, neu.PasswortHash!, "AltesPasswort123"));
    }

    [TestMethod]
    public void AnG3SuS_fuer_unbekannte_Id_liefert_false()
    {
        Assert.IsFalse(Dienst().AnG3SuS(99));
        Assert.IsEmpty(_email.Gesendet);
    }

    [TestMethod]
    public void AnAdministrationslogin_vergibt_ein_Passwort_ohne_Protokolleintrag()
    {
        var admin = _db.Admin("Ben", "Lehrer");

        var erfolg = Dienst().AnAdministrationslogin(admin.Id);

        Assert.IsTrue(erfolg);
        Assert.IsNotNull(_db.Administrationslogins.Single().PasswortHash);
        Assert.AreEqual(admin.Email, _email.Gesendet.Single().An);
        Assert.AreEqual(0, _db.EmailVersandListe.Count());
    }

    [TestMethod]
    public void AnAdministrationslogin_bei_Fehlschlag_laesst_das_Konto_unveraendert()
    {
        var admin = _db.Admin("Ben", "Lehrer");
        _email.Erfolg = false;

        Assert.IsFalse(Dienst().AnAdministrationslogin(admin.Id));
        Assert.IsNull(_db.Administrationslogins.Single().PasswortHash);
        Assert.IsFalse(Dienst().AnAdministrationslogin(99));
    }
}
