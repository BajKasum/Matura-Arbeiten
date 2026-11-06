using MaturaArbeiten.Domain;

namespace MaturaArbeiten.Tests;

[TestClass]
public class MeldungenTests
{
    private static readonly DateTime Jetzt = new(2026, 11, 6, 12, 0, 0, DateTimeKind.Utc);

    [TestMethod]
    public void Die_Sperrmeldung_nennt_die_Wartezeit_in_Minuten()
    {
        Assert.AreEqual(
            "Zu viele falsche Versuche. Bitte warte noch 15 Minuten.",
            Meldungen.Gesperrt(Jetzt.AddMinutes(15), Jetzt));
        Assert.AreEqual(
            "Zu viele falsche Versuche. Bitte warte noch 8 Minuten.",
            Meldungen.Gesperrt(Jetzt.AddMinutes(7).AddSeconds(10), Jetzt));
    }

    [TestMethod]
    public void Die_Sperrmeldung_sagt_bei_einer_Minute_eine_Minute()
    {
        Assert.AreEqual(
            "Zu viele falsche Versuche. Bitte warte noch eine Minute.",
            Meldungen.Gesperrt(Jetzt.AddSeconds(20), Jetzt));
    }

    [TestMethod]
    [DataRow(Abweisung.PhaseGeschlossen)]
    [DataRow(Abweisung.SchonZweiEintragungen)]
    [DataRow(Abweisung.PraesentationVoll)]
    [DataRow(Abweisung.Terminkollision)]
    public void Jeder_Abweisungsgrund_hat_eine_Meldung_fuer_G3SuS_und_Administration(Abweisung grund)
    {
        Assert.IsFalse(string.IsNullOrWhiteSpace(Meldungen.Fuer(grund)));
        Assert.IsFalse(string.IsNullOrWhiteSpace(Meldungen.FuerAdministration(grund)));
    }

    [TestMethod]
    public void Die_Meldung_bei_voller_Praesentation_fordert_zu_einer_anderen_Wahl_auf()
    {
        Assert.AreEqual(
            "Diese Präsentation ist inzwischen voll. Bitte wähle eine andere.",
            Meldungen.Fuer(Abweisung.PraesentationVoll));
    }
}
