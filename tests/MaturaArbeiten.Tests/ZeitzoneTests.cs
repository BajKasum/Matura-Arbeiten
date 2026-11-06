using MaturaArbeiten.Domain;

namespace MaturaArbeiten.Tests;

[TestClass]
public class ZeitzoneTests
{
    [TestMethod]
    public void Im_Winter_ist_Zuerich_eine_Stunde_vor_UTC()
    {
        Assert.AreEqual(new DateTime(2026, 12, 1, 11, 0, 0), Zeitzone.InZuerich(new DateTime(2026, 12, 1, 10, 0, 0)));
    }

    [TestMethod]
    public void Im_Sommer_ist_Zuerich_zwei_Stunden_vor_UTC()
    {
        Assert.AreEqual(new DateTime(2026, 7, 1, 12, 0, 0), Zeitzone.InZuerich(new DateTime(2026, 7, 1, 10, 0, 0)));
    }
}
