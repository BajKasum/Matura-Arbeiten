namespace MaturaArbeiten.Domain;

public static class EmailProtokoll
{
    public static IReadOnlyList<ProtokollEintrag> OffeneFehlschlaege(IEnumerable<ProtokollEintrag> eintraege) =>
        eintraege
            .GroupBy(e => (e.G3susId, e.Typ))
            .Select(gruppe => gruppe.OrderByDescending(e => e.Zeit).ThenByDescending(e => e.Id).First())
            .Where(e => e.Status == "Fehlgeschlagen")
            .OrderBy(e => e.Zeit)
            .ThenBy(e => e.Id)
            .ToList();
}
