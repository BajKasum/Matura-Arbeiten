using MaturaArbeiten.Domain;

namespace MaturaArbeiten.Tests;

[TestClass]
public class RegelnTests
{
    private static readonly Termin Wunsch = new(new DateOnly(2027, 1, 12), new TimeOnly(8, 30));
    private static readonly Termin Anderer = new(new DateOnly(2027, 1, 12), new TimeOnly(9, 30));
    private static readonly Termin Drittes = new(new DateOnly(2027, 1, 13), new TimeOnly(8, 30));

    public static IEnumerable<object[]> AlleSechzehnFaelle()
    {
        foreach (var phaseOffen in new[] { true, false })
        {
            foreach (var schonZwei in new[] { false, true })
            {
                foreach (var voll in new[] { false, true })
                {
                    foreach (var kollision in new[] { false, true })
                    {
                        yield return [phaseOffen, schonZwei, voll, kollision, Erwartet(phaseOffen, schonZwei, voll, kollision)];
                    }
                }
            }
        }
    }

    [TestMethod]
    [DynamicData(nameof(AlleSechzehnFaelle))]
    public void Pruefe_folgt_in_allen_sechzehn_Faellen_der_festen_Reihenfolge(
        bool phaseOffen,
        bool schonZwei,
        bool voll,
        bool kollision,
        EintragungErgebnis erwartet)
    {
        var ergebnis = Regeln.Pruefe(phaseOffen, 6, Wunsch, voll ? 6 : 3, EigeneTermine(schonZwei, kollision));

        Assert.AreEqual(erwartet, ergebnis);
    }

    [TestMethod]
    public void Pruefe_laesst_den_letzten_freien_Platz_zu()
    {
        var ergebnis = Regeln.Pruefe(true, 6, Wunsch, 5, []);

        Assert.AreEqual(new EintragungErgebnis.Erfolg(), ergebnis);
    }

    [TestMethod]
    public void Pruefe_weist_die_siebte_Eintragung_ab()
    {
        var ergebnis = Regeln.Pruefe(true, 6, Wunsch, 6, []);

        Assert.AreEqual(new EintragungErgebnis.Abgewiesen(Abweisung.PraesentationVoll), ergebnis);
    }

    [TestMethod]
    public void Pruefe_erlaubt_eine_Eintragung_zu_einer_anderen_Zeit()
    {
        var ergebnis = Regeln.Pruefe(true, 6, Wunsch, 0, [Anderer]);

        Assert.AreEqual(new EintragungErgebnis.Erfolg(), ergebnis);
    }

    [TestMethod]
    public void Pruefe_weist_dieselbe_Praesentation_zweimal_als_Terminkollision_ab()
    {
        var ergebnis = Regeln.Pruefe(true, 6, Wunsch, 1, [Wunsch]);

        Assert.AreEqual(new EintragungErgebnis.Abgewiesen(Abweisung.Terminkollision), ergebnis);
    }

    [TestMethod]
    [DataRow(6, 0, 6)]
    [DataRow(6, 4, 2)]
    [DataRow(6, 6, 0)]
    [DataRow(1, 1, 0)]
    public void FreiePlaetze_zieht_die_belegten_Plaetze_ab(int kontingent, int belegt, int erwartet)
    {
        Assert.AreEqual(erwartet, Regeln.FreiePlaetze(kontingent, belegt));
    }

    [TestMethod]
    public void Kollidiert_ist_nur_bei_gleichem_Datum_und_gleicher_Uhrzeit_wahr()
    {
        Assert.IsTrue(Regeln.Kollidiert(Wunsch, Wunsch));
        Assert.IsFalse(Regeln.Kollidiert(Wunsch, Anderer));
        Assert.IsFalse(Regeln.Kollidiert(Wunsch, Drittes));
    }

    [TestMethod]
    public void NochErreichbar_ist_wahr_mit_zwei_Eintragungen()
    {
        Assert.IsTrue(Regeln.NochErreichbar([Wunsch, Anderer], []));
    }

    [TestMethod]
    public void NochErreichbar_braucht_ohne_Eintragung_zwei_verschiedene_Zeiten()
    {
        Assert.IsFalse(Regeln.NochErreichbar([], []));
        Assert.IsFalse(Regeln.NochErreichbar([], [Wunsch]));
        Assert.IsFalse(Regeln.NochErreichbar([], [Wunsch, Wunsch]));
        Assert.IsTrue(Regeln.NochErreichbar([], [Wunsch, Anderer]));
    }

    [TestMethod]
    public void NochErreichbar_braucht_mit_einer_Eintragung_noch_eine_Moeglichkeit()
    {
        Assert.IsFalse(Regeln.NochErreichbar([Wunsch], []));
        Assert.IsTrue(Regeln.NochErreichbar([Wunsch], [Anderer]));
    }

    private static Termin[] EigeneTermine(bool schonZwei, bool kollision) => (schonZwei, kollision) switch
    {
        (true, true) => [Wunsch, Drittes],
        (true, false) => [Anderer, Drittes],
        (false, true) => [Wunsch],
        (false, false) => []
    };

    private static EintragungErgebnis Erwartet(bool phaseOffen, bool schonZwei, bool voll, bool kollision) =>
        !phaseOffen ? new EintragungErgebnis.Abgewiesen(Abweisung.PhaseGeschlossen)
        : schonZwei ? new EintragungErgebnis.Abgewiesen(Abweisung.SchonZweiEintragungen)
        : voll ? new EintragungErgebnis.Abgewiesen(Abweisung.PraesentationVoll)
        : kollision ? new EintragungErgebnis.Abgewiesen(Abweisung.Terminkollision)
        : new EintragungErgebnis.Erfolg();
}
