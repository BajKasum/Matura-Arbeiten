using MaturaArbeiten.Domain;

namespace MaturaArbeiten.Data;

public static class AppDbContextErweiterungen
{
    public static bool EmailVergeben(
        this AppDbContext db,
        string email,
        int? ausserG3SuSId = null,
        int? ausserAdministrationsloginId = null)
    {
        var kleingeschrieben = email.ToLowerInvariant();

        return db.G3SuSListe.Any(g => g.Email.ToLower() == kleingeschrieben && g.Id != ausserG3SuSId)
            || db.Administrationslogins.Any(a => a.Email.ToLower() == kleingeschrieben && a.Id != ausserAdministrationsloginId);
    }

    public static IReadOnlyList<ProtokollEintrag> Protokoll(this AppDbContext db) =>
        db.EmailVersandListe
            .Select(m => new { m.Id, m.G3susId, m.Typ, m.Status, m.GesendetAm })
            .ToList()
            .Select(m => new ProtokollEintrag(m.Id, m.G3susId, m.Typ, m.Status, m.GesendetAm))
            .ToList();
}
