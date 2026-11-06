namespace MaturaArbeiten.Domain;

public static class Mailtexte
{
    public static Mail Zugangsdaten(string vorname, string email, string passwort) =>
        new(
            email,
            "Deine Zugangsdaten für die Zusehendenverwaltung",
            $"""
            Hallo {vorname}

            hier sind deine Zugangsdaten für die Zusehendenverwaltung.

            Benutzername: {email}
            Passwort: {passwort}

            Mit diesen Daten wählst du deine zwei Präsentationen.
            """);

    public static Mail ZugangsdatenAdministration(string vorname, string email, string passwort) =>
        new(
            email,
            "Deine Zugangsdaten für die Zusehendenverwaltung",
            $"""
            Hallo {vorname}

            du hast ein Administrationslogin für die Zusehendenverwaltung bekommen.

            Benutzername: {email}
            Passwort: {passwort}
            """);
}
