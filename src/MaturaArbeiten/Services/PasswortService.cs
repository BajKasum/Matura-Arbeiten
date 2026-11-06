using MaturaArbeiten.Data;
using MaturaArbeiten.Domain;
using Microsoft.AspNetCore.Identity;

namespace MaturaArbeiten.Services;

public sealed class PasswortService(
    AppDbContext db,
    IAnmeldeService anmeldeService,
    IPasswordHasher<string> hasher,
    TimeProvider zeit) : IPasswortService
{
    public string? Aendern(string rolle, int id, string aktuell, string neu)
    {
        var email = rolle == Rollen.Administration
            ? db.Administrationslogins.Where(a => a.Id == id).Select(a => a.Email).SingleOrDefault()
            : db.G3SuSListe.Where(g => g.Id == id).Select(g => g.Email).SingleOrDefault();

        if (email is null)
        {
            return "Dieses Konto gibt es nicht mehr.";
        }

        switch (anmeldeService.Anmelden(email, aktuell))
        {
            case AnmeldeErgebnis.Erfolg:
                break;

            case AnmeldeErgebnis.Gesperrt gesperrt:
                return Meldungen.Gesperrt(gesperrt.Bis, zeit.GetUtcNow().UtcDateTime);

            default:
                return "Das bisherige Passwort stimmt nicht.";
        }

        var hash = hasher.HashPassword(email, neu);

        if (rolle == Rollen.Administration)
        {
            db.Administrationslogins.Single(a => a.Id == id).PasswortHash = hash;
        }
        else
        {
            var schueler = db.G3SuSListe.Single(g => g.Id == id);
            schueler.PasswortHash = hash;
            schueler.PasswortGeaendert = true;
        }

        db.SaveChanges();

        return null;
    }
}
