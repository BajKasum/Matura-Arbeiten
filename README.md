# Zusehendenverwaltung

Web-Applikation für das Wirtschaftsgymnasium Basel. G3-Schülerinnen und -Schüler tragen sich hier als Zusehende für Maturaarbeits-Präsentationen der G4 ein. Die Applikation ersetzt ein Formular in Microsoft Teams, das Überbelegungen nicht verhindert hat.

Das Projekt entsteht als IMS Projekt 1 an der IMS Basel (Klasse i3-26).

## Worum es geht

Es gibt zwei Rollen:

| Rolle | Aufgaben |
|---|---|
| G3-SuS | Präsentationen mit freien Plätzen ansehen, genau zwei auswählen, die eigenen Eintragungen mit Datum, Uhrzeit und Raum ansehen |
| Administration | Präsentationen und G3-SuS per CSV importieren, E-Mails auslösen, Listen erzeugen, Eintragungen anpassen, Eintragungsphase öffnen und schliessen, das System für ein neues Schuljahr zurücksetzen |

Diese Regeln prüft die Applikation schon bei der Auswahl:

- Jede Person wählt genau zwei Präsentationen.
- Pro Präsentation gibt es höchstens sechs Zusehende.
- Zwei Präsentationen zur gleichen Zeit sind nicht möglich.
- Nur die Administration kann Eintragungen löschen.

## Technik

| Bereich | Verwendet |
|---|---|
| Sprache und Framework | C#, ASP.NET Core MVC, .NET 10 |
| Oberfläche | Razor-Views, Bootstrap 5.3 |
| Datenbank | SQL Server (lokal SQL Server Express) |
| Datenzugriff | Entity Framework Core 10 |
| Versionsverwaltung | Git, Bitbucket (offiziell), GitHub (Spiegel) |

## Stand

| Bereich | Stand |
|---|---|
| Grundgerüst (ASP.NET Core, Datenbankanbindung, Git) | fertig |
| Oberfläche Desktop (Anmeldung, Präsentationen auswählen, Meine Eintragungen, Administration) | fertig, aber statisch: Die Daten stehen fest im HTML, die Buttons haben noch keine Funktion |
| Datenmodell | In Arbeit: Die sechs Klassen sind geschrieben, `AppDbContext`, Migration und Tabellen folgen |
| Regeln, Anmeldung, CSV-Import, Eintragen, Administration, E-Mail-Versand, Listen | offen |
| Handy-Ansicht und Tests | offen |

## Projekt starten

### Voraussetzungen

- .NET 10 SDK
- SQL Server Express mit der Instanz `.\SQLEXPRESS`
- EF Core Tools:

```bash
dotnet tool install --global dotnet-ef
```

### Schritte

Repository klonen und in den Projektordner wechseln:

```bash
git clone https://bitbucket.org/ims_i3b/matura-arbeiten.git
```

```bash
cd matura-arbeiten/src/MaturaArbeiten
```

Datenbank anlegen und Migrationen anwenden:

```bash
dotnet ef database update
```

Applikation starten:

```bash
dotnet run
```

Die Applikation läuft dann unter `http://localhost:5283`. Mit dem Profil `https` läuft sie zusätzlich unter `https://localhost:7234`:

```bash
dotnet run --launch-profile https
```

Die Startseite leitet zur Anmeldung weiter. Diese Seiten gibt es bisher:

| Seite | Adresse |
|---|---|
| Anmeldung | `/Account/Anmelden` |
| Präsentationen auswählen | `/Praesentation` |
| Meine Eintragungen | `/Eintragung/MeineEintragungen` |
| Administration | `/Admin` |

### Datenbank

Der Connection String steht in `src/MaturaArbeiten/appsettings.json`. Er verwendet die Windows-Anmeldung, es ist kein Passwort nötig:

```text
Server=.\SQLEXPRESS;Database=MaturaArbeitenDb;Trusted_Connection=True;TrustServerCertificate=True;
```

Wer eine andere SQL-Server-Instanz benutzt, ändert dort den Servernamen.

## Dokumente

Im Ordner `Docs/` liegen die Unterlagen der Abgabe:

- Lastenheft
- Software Design
- Anforderungsliste (Excel)

## Arbeitsweise mit Git

- Gearbeitet wird nie direkt auf `main`, sondern für jedes Feature in einem eigenen Branch (`feature/...`).
- Fertige Branches kommen über einen Pull Request in Bitbucket nach `main`.
- Alle Branches bleiben bestehen, damit nachvollziehbar ist, wie das Projekt entstanden ist.
