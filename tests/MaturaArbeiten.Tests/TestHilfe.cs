using MaturaArbeiten.Data;
using MaturaArbeiten.Domain;
using MaturaArbeiten.Models;
using MaturaArbeiten.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

namespace MaturaArbeiten.Tests;

public sealed class TestZeit : TimeProvider
{
    private DateTimeOffset _jetzt = new(2026, 11, 6, 12, 0, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => _jetzt;

    public void Vor(TimeSpan dauer) => _jetzt += dauer;
}

public sealed class TestEmail : IEmailService
{
    public bool Erfolg { get; set; } = true;

    public List<Mail> Gesendet { get; } = [];

    public bool Senden(Mail mail)
    {
        if (Erfolg)
        {
            Gesendet.Add(mail);
        }

        return Erfolg;
    }
}

public static class TestDb
{
    public static readonly PasswordHasher<string> Hasher = new();

    public static AppDbContext Neu()
    {
        var optionen = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var db = new AppDbContext(optionen);
        db.Database.EnsureCreated();

        return db;
    }

    public static G3SuS Schueler(
        this AppDbContext db,
        string vorname,
        string nachname,
        string? passwort = null)
    {
        var email = $"{vorname}.{nachname}@stud.edubs.ch".ToLowerInvariant();

        var schueler = new G3SuS
        {
            Vorname = vorname,
            Nachname = nachname,
            Email = email,
            PasswortHash = passwort is null ? null : Hasher.HashPassword(email, passwort)
        };

        db.G3SuSListe.Add(schueler);
        db.SaveChanges();

        return schueler;
    }

    public static Administrationslogin Admin(
        this AppDbContext db,
        string vorname,
        string nachname,
        string? passwort = null)
    {
        var email = $"{vorname}.{nachname}@schule.ch".ToLowerInvariant();

        var login = new Administrationslogin
        {
            Vorname = vorname,
            Nachname = nachname,
            Email = email,
            PasswortHash = passwort is null ? null : Hasher.HashPassword(email, passwort)
        };

        db.Administrationslogins.Add(login);
        db.SaveChanges();

        return login;
    }

    public static Praesentation NeuePraesentation(
        this AppDbContext db,
        string thema,
        int tag,
        int stunde,
        string raum = "204")
    {
        var praesentation = new Praesentation
        {
            Thema = thema,
            Datum = new DateOnly(2027, 1, tag),
            Uhrzeit = new TimeOnly(stunde, 0),
            Raum = raum
        };

        db.Praesentationen.Add(praesentation);
        db.SaveChanges();

        return praesentation;
    }

    public static Eintragung Eintragen(this AppDbContext db, G3SuS schueler, Praesentation praesentation)
    {
        var eintragung = new Eintragung
        {
            G3susId = schueler.Id,
            PraesentationId = praesentation.Id,
            ErstelltAm = DateTime.UtcNow
        };

        db.Eintragungen.Add(eintragung);
        db.SaveChanges();

        return eintragung;
    }

    public static void SetzeKontingent(this AppDbContext db, int platzkontingent)
    {
        db.Systemeinstellungen.Single().Platzkontingent = platzkontingent;
        db.SaveChanges();
    }

    public static void SetzePhase(this AppDbContext db, bool offen)
    {
        db.Systemeinstellungen.Single().EintragungsphaseOffen = offen;
        db.SaveChanges();
    }

    public static MemoryStream Csv(string inhalt) =>
        new([.. new byte[] { 0xEF, 0xBB, 0xBF }, .. System.Text.Encoding.UTF8.GetBytes(inhalt)]);
}
