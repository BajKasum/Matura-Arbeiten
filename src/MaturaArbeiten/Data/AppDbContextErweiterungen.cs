using MaturaArbeiten.Domain;
using Microsoft.Data.SqlClient;

namespace MaturaArbeiten.Data;

public static class AppDbContextErweiterungen
{
    private const int MaxVersuche = 2;
    private const int MinWartezeit = 20;
    private const int MaxWartezeit = 150;
    private static readonly int[] KonfliktNummern = [1205, 2601, 2627];

    public static T? Versuchen<T>(this AppDbContext db, Func<T> aktion) where T : class
    {
        for (var versuch = 1; versuch <= MaxVersuche; versuch++)
        {
            try
            {
                return aktion();
            }
            catch (Exception fehler) when (IstKonflikt(fehler))
            {
                db.ChangeTracker.Clear();

                if (versuch < MaxVersuche)
                {
                    Thread.Sleep(Random.Shared.Next(MinWartezeit, MaxWartezeit));
                }
            }
        }

        return null;
    }

    private static bool IstKonflikt(Exception? fehler) =>
        fehler is not null
        && (fehler is SqlException sql && KonfliktNummern.Contains(sql.Number)
            || IstKonflikt(fehler.InnerException));

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
