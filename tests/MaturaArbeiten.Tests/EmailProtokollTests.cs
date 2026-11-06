using MaturaArbeiten.Domain;

namespace MaturaArbeiten.Tests;

[TestClass]
public class EmailProtokollTests
{
    private static readonly DateTime T = new(2026, 11, 6, 12, 0, 0, DateTimeKind.Utc);

    [TestMethod]
    public void Ein_Fehlschlag_ohne_spaeteren_Erfolg_bleibt_offen()
    {
        var offene = EmailProtokoll.OffeneFehlschlaege([new ProtokollEintrag(1, 7, "Zugangsdaten", "Fehlgeschlagen", T)]);

        Assert.HasCount(1, offene);
        Assert.AreEqual(1, offene[0].Id);
    }

    [TestMethod]
    public void Ein_spaeterer_Erfolg_schliesst_den_Fehlschlag()
    {
        var offene = EmailProtokoll.OffeneFehlschlaege(
        [
            new ProtokollEintrag(1, 7, "Zugangsdaten", "Fehlgeschlagen", T),
            new ProtokollEintrag(2, 7, "Zugangsdaten", "Gesendet", T.AddMinutes(5))
        ]);

        Assert.IsEmpty(offene);
    }

    [TestMethod]
    public void Ein_neuer_Fehlschlag_nach_einem_Erfolg_ist_wieder_offen()
    {
        var offene = EmailProtokoll.OffeneFehlschlaege(
        [
            new ProtokollEintrag(1, 7, "Bestaetigung", "Gesendet", T),
            new ProtokollEintrag(2, 7, "Bestaetigung", "Fehlgeschlagen", T.AddMinutes(5))
        ]);

        Assert.HasCount(1, offene);
        Assert.AreEqual(2, offene[0].Id);
    }

    [TestMethod]
    public void Zugangsdaten_und_Bestaetigung_werden_getrennt_betrachtet()
    {
        var offene = EmailProtokoll.OffeneFehlschlaege(
        [
            new ProtokollEintrag(1, 7, "Zugangsdaten", "Gesendet", T),
            new ProtokollEintrag(2, 7, "Bestaetigung", "Fehlgeschlagen", T.AddMinutes(1))
        ]);

        Assert.HasCount(1, offene);
        Assert.AreEqual("Bestaetigung", offene[0].Typ);
    }

    [TestMethod]
    public void Die_offenen_Fehlschlaege_sind_nach_Zeit_sortiert()
    {
        var offene = EmailProtokoll.OffeneFehlschlaege(
        [
            new ProtokollEintrag(1, 8, "Zugangsdaten", "Fehlgeschlagen", T.AddMinutes(9)),
            new ProtokollEintrag(2, 7, "Zugangsdaten", "Fehlgeschlagen", T.AddMinutes(1))
        ]);

        CollectionAssert.AreEqual(new[] { 2, 1 }, offene.Select(e => e.Id).ToArray());
    }
}
