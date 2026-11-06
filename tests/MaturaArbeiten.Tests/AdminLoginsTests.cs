using MaturaArbeiten.Domain;

namespace MaturaArbeiten.Tests;

[TestClass]
public class AdminLoginsTests
{
    [TestMethod]
    public void Das_letzte_Login_darf_nicht_geloescht_werden()
    {
        Assert.IsFalse(AdminLogins.DarfGeloeschtWerden(1));
        Assert.IsFalse(AdminLogins.DarfGeloeschtWerden(0));
    }

    [TestMethod]
    public void Bei_mehreren_Logins_darf_eines_geloescht_werden()
    {
        Assert.IsTrue(AdminLogins.DarfGeloeschtWerden(2));
    }
}
