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
}
