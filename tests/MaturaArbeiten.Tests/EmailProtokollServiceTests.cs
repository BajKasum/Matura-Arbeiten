using MaturaArbeiten.Data;
using MaturaArbeiten.Domain;
using MaturaArbeiten.Services;

namespace MaturaArbeiten.Tests;

[TestClass]
public class EmailProtokollServiceTests
{
    private readonly TestZeit _zeit = new();
    private readonly TestEmail _email = new();
    private readonly AppDbContext _db = TestDb.Neu();

    private EmailProtokollService Dienst() =>
        new(
            _db,
            new ZugangsdatenService(_db, _email, TestDb.Hasher, _zeit),
            new BestaetigungService(_db, _email, _zeit));

    [TestMethod]
    public void Fehlgeschlagene_zeigt_Namen_Typ_und_Zeit()
    {
        _db.Schueler("Anna", "Muster");
        _email.Erfolg = false;
        new ZugangsdatenService(_db, _email, TestDb.Hasher, _zeit).Versenden();

        var zeile = Dienst().Fehlgeschlagene().Zeilen.Single();

        Assert.AreEqual("Muster Anna", zeile.G3SuS);
        Assert.AreEqual("Zugangsdaten", zeile.Typ);
        Assert.AreEqual(_zeit.GetUtcNow().UtcDateTime, zeile.Zeit);
    }

    [TestMethod]
    public void ErneutSenden_von_Zugangsdaten_loest_den_Fehlschlag_auf()
    {
        _db.Schueler("Anna", "Muster");
        _email.Erfolg = false;
        new ZugangsdatenService(_db, _email, TestDb.Hasher, _zeit).Versenden();
        _email.Erfolg = true;
        _zeit.Vor(TimeSpan.FromMinutes(1));
        var protokollId = Dienst().Fehlgeschlagene().Zeilen.Single().Id;

        var erfolg = Dienst().ErneutSenden(protokollId);

        Assert.IsTrue(erfolg);
        Assert.IsEmpty(Dienst().Fehlgeschlagene().Zeilen);
        Assert.IsNotNull(_db.G3SuSListe.Single().PasswortHash);
    }

    [TestMethod]
    public void ErneutSenden_von_Bestaetigungen_funktioniert()
    {
        var anna = _db.Schueler("Anna", "Muster");
        _db.Eintragen(anna, _db.NeuePraesentation("Eins", 12, 8));
        _db.Eintragen(anna, _db.NeuePraesentation("Zwei", 12, 9));
        _email.Erfolg = false;
        new BestaetigungService(_db, _email, _zeit).Senden(anna.Id);
        _email.Erfolg = true;
        _zeit.Vor(TimeSpan.FromMinutes(1));

        var (gesendet, fehlgeschlagen) = Dienst().AlleErneutSenden();

        Assert.AreEqual(1, gesendet);
        Assert.AreEqual(0, fehlgeschlagen);
        Assert.IsEmpty(Dienst().Fehlgeschlagene().Zeilen);
    }

    [TestMethod]
    public void ErneutSenden_einer_unbekannten_oder_erledigten_Meldung_liefert_false()
    {
        Assert.IsFalse(Dienst().ErneutSenden(99));
    }

    [TestMethod]
    public void AlleErneutSenden_meldet_weiter_fehlschlagende_Mails()
    {
        _db.Schueler("Anna", "Muster");
        _email.Erfolg = false;
        new ZugangsdatenService(_db, _email, TestDb.Hasher, _zeit).Versenden();
        _zeit.Vor(TimeSpan.FromMinutes(1));

        var (gesendet, fehlgeschlagen) = Dienst().AlleErneutSenden();

        Assert.AreEqual(0, gesendet);
        Assert.AreEqual(1, fehlgeschlagen);
        Assert.HasCount(1, Dienst().Fehlgeschlagene().Zeilen);
    }
}
