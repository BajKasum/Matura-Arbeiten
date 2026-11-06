using MaturaArbeiten.Domain;

namespace MaturaArbeiten.Tests;

[TestClass]
public class MailtexteTests
{
    [TestMethod]
    public void Die_Zugangsdaten_nennen_Benutzername_und_Passwort()
    {
        var mail = Mailtexte.Zugangsdaten("Anna", "anna@stud.edubs.ch", "Geheim12345");

        Assert.AreEqual("anna@stud.edubs.ch", mail.An);
        Assert.Contains("Hallo Anna", mail.Text);
        Assert.Contains("Benutzername: anna@stud.edubs.ch", mail.Text);
        Assert.Contains("Passwort: Geheim12345", mail.Text);
    }

    [TestMethod]
    public void Die_Bestaetigung_nennt_beide_Praesentationen_mit_Zeit_und_Raum()
    {
        var mail = Mailtexte.Bestaetigung(
            "Anna",
            "anna@stud.edubs.ch",
            [
                new MailEintragung("Klimawandel", new DateOnly(2027, 1, 12), new TimeOnly(8, 30), "204"),
                new MailEintragung("Fairtrade", new DateOnly(2027, 1, 13), new TimeOnly(9, 0), "302")
            ]);

        Assert.Contains("12.01.2027, 08:30 Uhr, Raum 204: Klimawandel", mail.Text);
        Assert.Contains("13.01.2027, 09:00 Uhr, Raum 302: Fairtrade", mail.Text);
        Assert.DoesNotContain("Passwort", mail.Text);
    }
}
