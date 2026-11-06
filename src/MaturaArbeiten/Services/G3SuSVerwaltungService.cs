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

    public G3SuSFormular? Finden(int id) =>
        db.G3SuSListe
            .Where(g => g.Id == id)
            .Select(g => new G3SuSFormular { Vorname = g.Vorname, Nachname = g.Nachname, Email = g.Email })
            .SingleOrDefault();

    public string? Anlegen(G3SuSFormular eingabe)
    {
        var email = eingabe.Email.Trim();

        if (IstVergeben(email, null))
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

    public string? Aendern(int id, G3SuSFormular eingabe)
    {
        var schueler = db.G3SuSListe.SingleOrDefault(g => g.Id == id);

        if (schueler is null)
        {
            return "Diesen G3-SuS gibt es nicht mehr.";
        }

        var email = eingabe.Email.Trim();

        if (IstVergeben(email, id))
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

    private bool IstVergeben(string email, int? ausserG3SuSId) =>
        db.G3SuSListe.Any(g => g.Email == email && g.Id != ausserG3SuSId)
        || db.Administrationslogins.Any(a => a.Email == email);
}
