using MaturaArbeiten.Domain;

namespace MaturaArbeiten.Tests;

[TestClass]
public class SperreTests
{
    private static readonly DateTime Jetzt = new(2026, 11, 6, 12, 0, 0, DateTimeKind.Utc);

    [TestMethod]
    public void Ein_falsches_Passwort_zaehlt_hoch_und_sperrt_noch_nicht()
    {
        var konto = Sperre.NachFalschemPasswort(new Konto(0, null), Jetzt);

        Assert.AreEqual(new Konto(1, null), konto);
        Assert.IsFalse(Sperre.IstGesperrt(konto, Jetzt));
    }

    [TestMethod]
    public void Das_fuenfte_falsche_Passwort_sperrt_fuer_fuenfzehn_Minuten_und_setzt_den_Zaehler_zurueck()
    {
        var konto = Sperre.NachFalschemPasswort(new Konto(4, null), Jetzt);

        Assert.AreEqual(new Konto(0, Jetzt.AddMinutes(15)), konto);
        Assert.IsTrue(Sperre.IstGesperrt(konto, Jetzt));
        Assert.IsTrue(Sperre.IstGesperrt(konto, Jetzt.AddMinutes(14)));
    }

    [TestMethod]
    public void Nach_fuenfzehn_Minuten_ist_die_Sperre_vorbei()
    {
        var konto = new Konto(0, Jetzt.AddMinutes(15));

        Assert.IsFalse(Sperre.IstGesperrt(konto, Jetzt.AddMinutes(15)));
        Assert.IsFalse(Sperre.IstGesperrt(konto, Jetzt.AddMinutes(16)));
    }

    [TestMethod]
    public void Ein_falsches_Passwort_nach_Ablauf_der_Sperre_loescht_die_alte_Sperrzeit()
    {
        var abgelaufen = new Konto(0, Jetzt.AddMinutes(-1));

        var konto = Sperre.NachFalschemPasswort(abgelaufen, Jetzt);

        Assert.AreEqual(new Konto(1, null), konto);
    }

    [TestMethod]
    public void Ein_richtiges_Passwort_setzt_alles_zurueck()
    {
        Assert.AreEqual(new Konto(0, null), Sperre.NachRichtigemPasswort());
    }

    [TestMethod]
    public void Ein_Konto_ohne_Sperrzeit_ist_nicht_gesperrt()
    {
        Assert.IsFalse(Sperre.IstGesperrt(new Konto(3, null), Jetzt));
    }
}
