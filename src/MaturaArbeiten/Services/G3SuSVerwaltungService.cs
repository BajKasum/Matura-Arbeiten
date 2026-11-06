using MaturaArbeiten.Data;
using MaturaArbeiten.Models;
using MaturaArbeiten.ViewModels;

namespace MaturaArbeiten.Services;

public sealed class G3SuSVerwaltungService(AppDbContext db) : IG3SuSVerwaltungService
{
    private const string EmailVergeben = "Diese E-Mail-Adresse gibt es schon.";

    public IReadOnlyList<G3SuSZeile> Liste()
    {
        var anzahl = db.Eintragungen
            .GroupBy(e => e.G3susId)
            .Select(gruppe => new { Id = gruppe.Key, Anzahl = gruppe.Count() })
            .ToDictionary(x => x.Id, x => x.Anzahl);

        return db.G3SuSListe
            .OrderBy(g => g.Nachname)
            .ThenBy(g => g.Vorname)
            .ToList()
            .Select(g => new G3SuSZeile(
                g.Id,
                g.Vorname,
                g.Nachname,
                g.Email,
                anzahl.GetValueOrDefault(g.Id),
                g.PasswortHash is not null))
            .ToList();
    }

    public PersonFormular? Finden(int id) =>
        db.G3SuSListe
            .Where(g => g.Id == id)
            .Select(g => new PersonFormular { Vorname = g.Vorname, Nachname = g.Nachname, Email = g.Email })
            .SingleOrDefault();

    public string? Anlegen(PersonFormular eingabe)
    {
        var email = eingabe.Email.Trim();

        if (db.EmailVergeben(email))
        {
            return EmailVergeben;
        }

        db.G3SuSListe.Add(new G3SuS
        {
            Vorname = eingabe.Vorname.Trim(),
            Nachname = eingabe.Nachname.Trim(),
            Email = email
        });
        db.SaveChanges();

        return null;
    }

    public string? Aendern(int id, PersonFormular eingabe)
    {
        var schueler = db.G3SuSListe.SingleOrDefault(g => g.Id == id);

        if (schueler is null)
        {
            return "Diesen G3-SuS gibt es nicht mehr.";
        }

        var email = eingabe.Email.Trim();

        if (db.EmailVergeben(email, ausserG3SuSId: id))
        {
            return EmailVergeben;
        }

        schueler.Vorname = eingabe.Vorname.Trim();
        schueler.Nachname = eingabe.Nachname.Trim();
        schueler.Email = email;
        db.SaveChanges();

        return null;
    }

    public bool Loeschen(int id)
    {
        var schueler = db.G3SuSListe.SingleOrDefault(g => g.Id == id);

        if (schueler is null)
        {
            return false;
        }

        db.EmailVersandListe.RemoveRange(db.EmailVersandListe.Where(m => m.G3susId == id));
        db.Eintragungen.RemoveRange(db.Eintragungen.Where(e => e.G3susId == id));
        db.G3SuSListe.Remove(schueler);
        db.SaveChanges();

        return true;
    }
}
