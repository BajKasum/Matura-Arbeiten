namespace MaturaArbeiten.Domain;

public static class EmailProtokoll
{
    public const string Zugangsdaten = "Zugangsdaten";
    public const string Bestaetigung = "Bestaetigung";
    public const string Gesendet = "Gesendet";
    public const string Fehlgeschlagen = "Fehlgeschlagen";

    // Je G3-SuS und Typ zählt nur der neueste Eintrag: Ein Fehlschlag ist offen, bis ein späterer Versand gelingt.
    public static IReadOnlyList<ProtokollEintrag> OffeneFehlschlaege(IEnumerable<ProtokollEintrag> eintraege) =>
        eintraege
            .GroupBy(e => (e.G3susId, e.Typ))
            .Select(gruppe => gruppe.OrderByDescending(e => e.Zeit).ThenByDescending(e => e.Id).First())
            .Where(e => e.Status == Fehlgeschlagen)
            .OrderBy(e => e.Zeit)
            .ThenBy(e => e.Id)
            .ToList();
}
