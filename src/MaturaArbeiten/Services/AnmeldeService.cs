using MaturaArbeiten.Data;
using MaturaArbeiten.Domain;
using Microsoft.AspNetCore.Identity;

namespace MaturaArbeiten.Services;

public sealed class AnmeldeService(
    AppDbContext db,
    IPasswordHasher<string> hasher,
    TimeProvider zeit) : IAnmeldeService
{
    public AnmeldeErgebnis Anmelden(string email, string passwort)
    {
        var jetzt = zeit.GetUtcNow().UtcDateTime;

        var admin = db.Administrationslogins.SingleOrDefault(a => a.Email == email);
        if (admin is not null)
        {
            return Pruefe(
                email,
                passwort,
                admin.PasswortHash,
                new Konto(admin.Fehlversuche, admin.GesperrtBis),
                jetzt,
                neu =>
                {
                    admin.Fehlversuche = neu.Fehlversuche;
                    admin.GesperrtBis = neu.GesperrtBis;
                },
                () => new AnmeldeErgebnis.Erfolg(
                    admin.Id,
                    $"{admin.Vorname} {admin.Nachname}",
                    Rollen.Administration));
        }

        var schueler = db.G3SuSListe.SingleOrDefault(g => g.Email == email);
        if (schueler is not null)
        {
            return Pruefe(
                email,
                passwort,
                schueler.PasswortHash,
                new Konto(schueler.Fehlversuche, schueler.GesperrtBis),
                jetzt,
                neu =>
                {
                    schueler.Fehlversuche = neu.Fehlversuche;
                    schueler.GesperrtBis = neu.GesperrtBis;
                },
                () => new AnmeldeErgebnis.Erfolg(
                    schueler.Id,
                    $"{schueler.Vorname} {schueler.Nachname}",
                    Rollen.G3SuS));
        }

        return new AnmeldeErgebnis.Abgewiesen();
    }

    private AnmeldeErgebnis Pruefe(
        string email,
        string passwort,
        string? hash,
        Konto konto,
        DateTime jetzt,
        Action<Konto> uebernehmen,
        Func<AnmeldeErgebnis> erfolg)
    {
        if (Sperre.IstGesperrt(konto, jetzt))
        {
            return new AnmeldeErgebnis.Gesperrt(konto.GesperrtBis!.Value);
        }

        if (hash is null)
        {
            return new AnmeldeErgebnis.Abgewiesen();
        }

        var richtig = hasher.VerifyHashedPassword(email, hash, passwort) != PasswordVerificationResult.Failed;
        var neu = richtig ? Sperre.NachRichtigemPasswort() : Sperre.NachFalschemPasswort(konto, jetzt);

        uebernehmen(neu);
        db.SaveChanges();

        if (richtig)
        {
            return erfolg();
        }

        return Sperre.IstGesperrt(neu, jetzt)
            ? new AnmeldeErgebnis.Gesperrt(neu.GesperrtBis!.Value)
            : new AnmeldeErgebnis.Abgewiesen();
    }
}