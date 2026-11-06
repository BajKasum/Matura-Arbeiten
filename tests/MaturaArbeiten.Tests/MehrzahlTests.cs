using MaturaArbeiten.Domain;

namespace MaturaArbeiten.Tests;

[TestClass]
public class MehrzahlTests
{
    [TestMethod]
    [DataRow(0, "0 Eintragungen")]
    [DataRow(1, "1 Eintragung")]
    [DataRow(2, "2 Eintragungen")]
    [DataRow(25, "25 Eintragungen")]
    public void Von_waehlt_Einzahl_nur_bei_genau_eins(int anzahl, string erwartet)
    {
        Assert.AreEqual(erwartet, Mehrzahl.Von(anzahl, "Eintragung", "Eintragungen"));
    }
}
