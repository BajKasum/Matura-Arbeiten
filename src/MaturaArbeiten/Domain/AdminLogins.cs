namespace MaturaArbeiten.Domain;

public static class AdminLogins
{
    public static string? Loeschhindernis(
        int zuLoeschen,
        int angemeldet,
        IEnumerable<(int Id, bool HatPasswort)> logins) =>
        zuLoeschen == angemeldet
            ? "Du kannst dein eigenes Administrationslogin nicht löschen."
            : !logins.Any(login => login.Id != zuLoeschen && login.HatPasswort)
                ? "Es muss immer ein Administrationslogin mit Passwort übrig bleiben."
                : null;
}
