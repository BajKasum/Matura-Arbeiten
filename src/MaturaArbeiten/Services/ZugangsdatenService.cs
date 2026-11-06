using System.Security.Cryptography;
using MaturaArbeiten.Data;
using MaturaArbeiten.Domain;
using MaturaArbeiten.Models;
using Microsoft.AspNetCore.Identity;

namespace MaturaArbeiten.Services;

public sealed class ZugangsdatenService(
    AppDbContext db,
    IEmailService emailService,
    IPasswordHasher<string> hasher,
    TimeProvider zeit) : IZugangsdatenService
{
    private const string Zeichen = "abcdefghijkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public (int Gesendet, int Fehlgeschlagen) Versenden()
    {
        var jetzt = zeit.GetUtcNow().UtcDateTime;

        var ergebnisse = db.G3SuSListe
            .Where(g => g.PasswortHash == null)
            .ToList()
            .Select(g => Sende(g, jetzt))
            .ToList();

        db.SaveChanges();

        return (ergebnisse.Count(erfolg => erfolg), ergebnisse.Count(erfolg => !erfolg));
    }

    private bool Sende(G3SuS schueler, DateTime jetzt)
    {
        var passwort = NeuesPasswort();
        var erfolg = emailService.Senden(Mailtexte.Zugangsdaten(schueler.Vorname, schueler.Email, passwort));

        if (erfolg)
        {
            schueler.PasswortHash = hasher.HashPassword(schueler.Email, passwort);
            schueler.PasswortGeaendert = false;
            schueler.Fehlversuche = 0;
            schueler.GesperrtBis = null;
        }

        db.EmailVersandListe.Add(new EmailVersand
        {
            G3susId = schueler.Id,
            Typ = "Zugangsdaten",
            Status = erfolg ? "Gesendet" : "Fehlgeschlagen",
            GesendetAm = jetzt
        });

        return erfolg;
    }

    private static string NeuesPasswort() =>
        new(RandomNumberGenerator.GetItems<char>(Zeichen, 12));
}