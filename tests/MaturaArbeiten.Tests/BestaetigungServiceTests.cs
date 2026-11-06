using MaturaArbeiten.Data;
using MaturaArbeiten.Domain;
using MaturaArbeiten.Services;

namespace MaturaArbeiten.Tests;

[TestClass]
public class BestaetigungServiceTests
{
    private readonly TestZeit _zeit = new();
    private readonly TestEmail _email = new();
    private readonly AppDbContext _db = TestDb.Neu();

    private BestaetigungService Dienst() => new(_db, _email, _zeit);

    [TestMethod]
    public void Senden_verschickt_die_Bestaetigung_nach_Zeit_sortiert_und_protokolliert()
    {
        var anna = _db.Schueler("Anna", "Muster");
        _db.Eintragen(anna, _db.NeuePraesentation("Spaet", 13, 8));
        _db.Eintragen(anna, _db.NeuePraesentation("Frueh", 12, 8));

        var versand = Dienst().Senden(anna.Id);

        Assert.AreEqual(Versand.Gesendet, versand);
        var mail = _email.Gesendet.Single();
        Assert.AreEqual(anna.Email, mail.An);
        Assert.IsLessThan(mail.Text.IndexOf("Spaet"), mail.Text.IndexOf("Frueh"));
        var eintrag = _db.EmailVersandListe.Single();
        Assert.AreEqual("Bestaetigung", eintrag.Typ);
        Assert.AreEqual("Gesendet", eintrag.Status);
    }

    [TestMethod]
    public void Senden_mit_nur_einer_Eintragung_ist_nicht_noetig()
    {
        var anna = _db.Schueler("Anna", "Muster");
        _db.Eintragen(anna, _db.NeuePraesentation("Eins", 12, 8));

        Assert.AreEqual(Versand.NichtNoetig, Dienst().Senden(anna.Id));
        Assert.IsEmpty(_email.Gesendet);
        Assert.AreEqual(0, _db.EmailVersandListe.Count());
    }

    [TestMethod]
    public void Senden_an_einen_unbekannten_G3SuS_ist_nicht_noetig()
    {
        Assert.AreEqual(Versand.NichtNoetig, Dienst().Senden(99));
    }

    [TestMethod]
    public void Senden_bei_Fehlschlag_wird_protokolliert()
    {
        var anna = _db.Schueler("Anna", "Muster");
        _db.Eintragen(anna, _db.NeuePraesentation("Eins", 12, 8));
        _db.Eintragen(anna, _db.NeuePraesentation("Zwei", 12, 9));
        _email.Erfolg = false;

        Assert.AreEqual(Versand.Fehlgeschlagen, Dienst().Senden(anna.Id));
        Assert.AreEqual("Fehlgeschlagen", _db.EmailVersandListe.Single().Status);
    }

    [TestMethod]
    public void AnAlle_erreicht_nur_G3SuS_mit_erfuellter_Pflicht()
    {
        var anna = _db.Schueler("Anna", "Muster");
        var ben = _db.Schueler("Ben", "Beispiel");
        var eins = _db.NeuePraesentation("Eins", 12, 8);
        var zwei = _db.NeuePraesentation("Zwei", 12, 9);
        _db.Eintragen(anna, eins);
        _db.Eintragen(anna, zwei);
        _db.Eintragen(ben, eins);

        var (gesendet, fehlgeschlagen) = Dienst().AnAlle();

        Assert.AreEqual(1, gesendet);
        Assert.AreEqual(0, fehlgeschlagen);
        Assert.AreEqual(anna.Email, _email.Gesendet.Single().An);
    }
}
