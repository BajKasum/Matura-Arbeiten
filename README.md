# Zusehendenverwaltung

Web-Applikation für das Wirtschaftsgymnasium Basel. G3-Schülerinnen und -Schüler tragen sich hier als Zusehende für Maturaarbeits-Präsentationen der G4 ein. Die Applikation ersetzt ein Formular in Microsoft Teams, das Überbelegungen nicht verhindert hat.

Das Projekt entsteht als IMS Projekt 1 an der IMS Basel (Klasse i3-26).

## Worum es geht

Es gibt zwei Rollen:

| Rolle | Aufgaben |
|---|---|
| G3-SuS | Präsentationen mit freien Plätzen ansehen, genau zwei auswählen, die eigenen Eintragungen mit Datum, Uhrzeit und Raum ansehen, das Passwort ändern |
| Administration | Präsentationen und G3-SuS per CSV importieren, Zugangsdaten und Bestätigungen per E-Mail auslösen, Listen erzeugen, G3-SuS, Eintragungen und Administrationslogins verwalten, Eintragungsphase öffnen und schliessen, das System für ein neues Schuljahr zurücksetzen |

Diese Regeln prüft die Applikation schon bei der Auswahl:

- Jede Person wählt genau zwei Präsentationen.
- Pro Präsentation gibt es höchstens sechs Zusehende.
- Zwei Präsentationen zur gleichen Zeit sind nicht möglich.
- Nur die Administration kann Eintragungen ändern oder löschen.

## Technik

| Bereich | Verwendet |
|---|---|
| Sprache und Framework | C#, ASP.NET Core MVC, .NET 10 |
| Oberfläche | Razor-Views, Bootstrap 5.3 (ohne eigenes CSS) |
| Datenbank | SQL Server (lokal SQL Server Express) |
| Datenzugriff | Entity Framework Core 10 |
| Anmeldung | Cookie-Anmeldung, Passwörter mit PBKDF2, Sperre nach fünf falschen Versuchen |
| Tests | MSTest, Entity Framework Core InMemory |
| Versionsverwaltung | Git, Bitbucket (offiziell), GitHub (Spiegel) |

Aufbau von `src/MaturaArbeiten`:

| Ordner | Inhalt |
|---|---|
| `Domain` | Fachregeln als reine Funktionen und unveränderliche Records, ohne Datenbankzugriff |
| `Services` | Anwendungsfälle, die Daten laden, die Regeln aufrufen und speichern |
| `Controllers`, `Views`, `ViewModels` | Oberfläche |
| `Models`, `Data`, `Migrations` | Datenmodell, `AppDbContext` und Datenbankschema |

## Stand

| Bereich | Stand |
|---|---|
| Grundgerüst, Datenbank, Migrationen | fertig |
| Regeln, Anmeldung, Passwort ändern | fertig |
| CSV-Import, Eintragen, Meine Eintragungen | fertig |
| Administration (Übersicht, Verwaltung, Listen, Zurücksetzen) | fertig |
| E-Mails (Zugangsdaten, Bestätigung, E-Mail-Protokoll) | fertig, aber nur mit Konsolenausgabe: Ein echter E-Mail-Dienst ist noch nicht angebunden |
| Handy-Ansicht, Tests | fertig |
| Veröffentlichung (Azure) | offen |

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

Das erste Administrationslogin kommt aus den User Secrets. Beim Start wird es angelegt, solange es noch kein Administrationslogin gibt:

```bash
dotnet user-secrets set "Administration:Email" "vorname.nachname@schule.ch"
```

```bash
dotnet user-secrets set "Administration:Vorname" "Vorname"
```

```bash
dotnet user-secrets set "Administration:Nachname" "Nachname"
```

```bash
dotnet user-secrets set "Administration:Passwort" "ein-langes-passwort"
```

Applikation starten:

```bash
dotnet run
```

Die Applikation läuft dann unter `http://localhost:5283`. Mit dem Profil `https` läuft sie zusätzlich unter `https://localhost:7234`:

```bash
dotnet run --launch-profile https
```

### Ablauf

1. Als Administration anmelden (`/Account/Anmelden`).
2. Unter "Übersicht" die Präsentationen und die G3-SuS als CSV-Datei laden. Die Dateien sind UTF-8, mit Strichpunkt getrennt und haben eine Kopfzeile:
   - Präsentationen: `Thema;Datum;Uhrzeit;Raum`, zum Beispiel `Klimawandel;12.01.2027;08:30;204`
   - G3-SuS: `Vorname;Nachname;E-Mail`
3. Zugangsdaten an die G3-SuS versenden. Jede Person bekommt ein eigenes Passwort und muss es bei der ersten Anmeldung ändern.
4. Die G3-SuS wählen ihre zwei Präsentationen, solange die Eintragungsphase offen ist.
5. Bestätigungen versenden, Listen ansehen oder herunterladen.

Im Entwicklungsmodus schreibt die Applikation E-Mails in die Konsole, auch mit dem Passwort. Ausserhalb davon verschickt sie nichts, solange kein E-Mail-Dienst eingerichtet ist.

### Seiten

| Seite | Adresse |
|---|---|
| Anmeldung | `/Account/Anmelden` |
| Passwort ändern | `/Account/PasswortAendern` |
| Präsentationen auswählen | `/Praesentation` |
| Meine Eintragungen | `/Eintragung/MeineEintragungen` |
| Administration | `/Admin` |

### Tests

Im Hauptordner des Repositorys:

```bash
dotnet test MaturaArbeiten.slnx
```

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
