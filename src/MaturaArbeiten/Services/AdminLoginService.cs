using MaturaArbeiten.Data;
using MaturaArbeiten.Domain;
using MaturaArbeiten.Models;
using MaturaArbeiten.ViewModels;

namespace MaturaArbeiten.Services;

public sealed class AdminLoginService(AppDbContext db) : IAdminLoginService
{
    private const string EmailVergeben = "Diese E-Mail-Adresse gibt es schon.";

    public IReadOnlyList<AdminLoginZeile> Liste() =>
        db.Administrationslogins
            .OrderBy(a => a.Nachname)
            .ThenBy(a => a.Vorname)
            .ToList()
            .Select(a => new AdminLoginZeile(a.Id, a.Vorname, a.Nachname, a.Email, a.PasswortHash is not null))
            .ToList();

    public PersonFormular? Finden(int id) =>
        db.Administrationslogins
            .Where(a => a.Id == id)
            .Select(a => new PersonFormular { Vorname = a.Vorname, Nachname = a.Nachname, Email = a.Email })
            .SingleOrDefault();

    public (int? Id, string? Fehler) Anlegen(PersonFormular eingabe)
    {
        var email = eingabe.Email.Trim();

        if (db.EmailVergeben(email))
        {
            return (null, EmailVergeben);
        }

        var login = new Administrationslogin
        {
            Vorname = eingabe.Vorname.Trim(),
            Nachname = eingabe.Nachname.Trim(),
            Email = email
        };

        db.Administrationslogins.Add(login);
        db.SaveChanges();

        return (login.Id, null);
    }

    public string? Aendern(int id, PersonFormular eingabe)
    {
        var login = db.Administrationslogins.SingleOrDefault(a => a.Id == id);

        if (login is null)
        {
            return "Dieses Administrationslogin gibt es nicht mehr.";
        }

        var email = eingabe.Email.Trim();

        if (db.EmailVergeben(email, ausserAdministrationsloginId: id))
        {
            return EmailVergeben;
        }

        login.Vorname = eingabe.Vorname.Trim();
        login.Nachname = eingabe.Nachname.Trim();
        login.Email = email;
        db.SaveChanges();

        return null;
    }

    public string? Loeschen(int id, int angemeldetId)
    {
        var login = db.Administrationslogins.SingleOrDefault(a => a.Id == id);

        if (login is null)
        {
            return "Dieses Administrationslogin gibt es nicht mehr.";
        }

        var logins = db.Administrationslogins
            .Select(a => new { a.Id, HatPasswort = a.PasswortHash != null })
            .ToList()
            .Select(a => (a.Id, a.HatPasswort))
            .ToList();

        var hindernis = AdminLogins.Loeschhindernis(id, angemeldetId, logins);

        if (hindernis is not null)
        {
            return hindernis;
        }

        db.Administrationslogins.Remove(login);
        db.SaveChanges();

        return null;
    }
}
