namespace MaturaArbeiten.Domain;

public static class Mailtexte
{
    public static Mail Zugangsdaten(string vorname, string email, string passwort, string? adresse) =>
        new(
            email,
            "Deine Zugangsdaten für die Zusehendenverwaltung",
            $"""
            Hallo {vorname}

            hier sind deine Zugangsdaten für die Zusehendenverwaltung.

            {AdressZeile(adresse)}Benutzername: {email}
            Passwort: {passwort}

            Bei der ersten Anmeldung musst du dieses Passwort ändern.

            Mit diesen Daten wählst du deine zwei Präsentationen.
            """);

    public static Mail ZugangsdatenAdministration(string vorname, string email, string passwort, string? adresse) =>
        new(
            email,
            "Deine Zugangsdaten für die Zusehendenverwaltung",
            $"""
            Hallo {vorname}

            du hast ein Administrationslogin für die Zusehendenverwaltung bekommen.

            {AdressZeile(adresse)}Benutzername: {email}
            Passwort: {passwort}
            """);

    private static string AdressZeile(string? adresse) =>
        string.IsNullOrWhiteSpace(adresse) ? string.Empty : $"Adresse: {adresse}\n";

    public static Mail Bestaetigung(string vorname, string email, IEnumerable<MailEintragung> eintragungen) =>
        new(
            email,
            "Deine Bestätigung für die Zusehendenverwaltung",
            $"Hallo {vorname}\n\n"
            + "du hast dich für diese Präsentationen eingetragen:\n\n"
            + string.Join(
                "\n",
                eintragungen.Select(e => $"{e.Datum:dd.MM.yyyy}, {e.Uhrzeit:HH:mm} Uhr, Raum {e.Raum}: {e.Thema}"))
            + "\n\nDamit hast du den Pflichtbesuch erfüllt.");
}
