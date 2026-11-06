using MaturaArbeiten.Data;
using MaturaArbeiten.Domain;
using MaturaArbeiten.ViewModels;

namespace MaturaArbeiten.Services;

public sealed class EmailProtokollService(
    AppDbContext db,
    IZugangsdatenService zugangsdatenService,
    IBestaetigungService bestaetigungService) : IEmailProtokollService
{
    public EmailProtokollViewModel Fehlgeschlagene()
    {
        var namen = db.G3SuSListe
            .ToList()
            .ToDictionary(g => g.Id, g => $"{g.Nachname} {g.Vorname}");

        var zeilen = EmailProtokoll.OffeneFehlschlaege(db.Protokoll())
            .Select(e => new FehlgeschlagenZeile(
                e.Id,
                namen.GetValueOrDefault(e.G3susId, "unbekannt"),
                e.Typ,
                e.Zeit))
            .ToList();

        return new EmailProtokollViewModel(zeilen);
    }

    public bool ErneutSenden(int protokollId)
    {
        var eintrag = EmailProtokoll.OffeneFehlschlaege(db.Protokoll())
            .SingleOrDefault(e => e.Id == protokollId);

        if (eintrag is null)
        {
            return false;
        }

        return eintrag.Typ switch
        {
            "Zugangsdaten" => zugangsdatenService.AnG3SuS(eintrag.G3susId),
            "Bestaetigung" => bestaetigungService.Senden(eintrag.G3susId) == Versand.Gesendet,
            _ => false
        };
    }

    public (int Gesendet, int Fehlgeschlagen) AlleErneutSenden()
    {
        var ergebnisse = EmailProtokoll.OffeneFehlschlaege(db.Protokoll())
            .Select(e => e.Id)
            .ToList()
            .Select(ErneutSenden)
            .ToList();

        return (ergebnisse.Count(erfolg => erfolg), ergebnisse.Count(erfolg => !erfolg));
    }
}
