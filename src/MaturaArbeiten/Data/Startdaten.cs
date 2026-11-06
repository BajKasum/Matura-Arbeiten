using MaturaArbeiten.Models;
using Microsoft.AspNetCore.Identity;

namespace MaturaArbeiten.Data;

public static class Startdaten
{
    public static void ErstesAdministrationslogin(IServiceProvider dienste)
    {
        var db = dienste.GetRequiredService<AppDbContext>();
        var konfiguration = dienste.GetRequiredService<IConfiguration>();
        var hasher = dienste.GetRequiredService<IPasswordHasher<string>>();

        if (db.Administrationslogins.Any())
        {
            return;
        }

        var email = konfiguration["Administration:Email"];
        var vorname = konfiguration["Administration:Vorname"];
        var nachname = konfiguration["Administration:Nachname"];
        var passwort = konfiguration["Administration:Passwort"];

        if (string.IsNullOrWhiteSpace(email)
            || string.IsNullOrWhiteSpace(vorname)
            || string.IsNullOrWhiteSpace(nachname)
            || string.IsNullOrWhiteSpace(passwort))
        {
            return;
        }

        db.Administrationslogins.Add(new Administrationslogin
        {
            Vorname = vorname,
            Nachname = nachname,
            Email = email,
            PasswortHash = hasher.HashPassword(email, passwort)
        });
        db.SaveChanges();
    }
}