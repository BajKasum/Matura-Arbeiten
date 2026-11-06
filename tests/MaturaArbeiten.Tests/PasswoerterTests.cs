using MaturaArbeiten.Domain;

namespace MaturaArbeiten.Tests;

[TestClass]
public class PasswoerterTests
{
    [TestMethod]
    public void Ein_Passwort_unter_zehn_Zeichen_wird_abgewiesen()
    {
        Assert.IsNotNull(Passwoerter.Pruefe("alt", "kurz", "kurz"));
        Assert.IsNotNull(Passwoerter.Pruefe("alt", "123456789", "123456789"));
    }

    [TestMethod]
    public void Ein_Passwort_mit_zehn_Zeichen_ist_lang_genug()
    {
        Assert.IsNull(Passwoerter.Pruefe("alt", "1234567890", "1234567890"));
    }

    [TestMethod]
    public void Verschiedene_Wiederholung_wird_abgewiesen()
    {
        Assert.AreEqual(
            "Die beiden neuen Passwörter sind nicht gleich.",
            Passwoerter.Pruefe("alt", "EinLangesPasswort1", "EinLangesPasswort2"));
    }

    [TestMethod]
    public void Das_bisherige_Passwort_ist_als_neues_nicht_erlaubt()
    {
        Assert.AreEqual(
            "Das neue Passwort muss sich vom bisherigen unterscheiden.",
            Passwoerter.Pruefe("EinLangesPasswort1", "EinLangesPasswort1", "EinLangesPasswort1"));
    }
}
