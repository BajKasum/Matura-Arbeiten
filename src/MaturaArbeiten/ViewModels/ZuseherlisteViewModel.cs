namespace MaturaArbeiten.ViewModels;

public sealed record ZuseherlisteViewModel(
    IReadOnlyList<Auswahl> Praesentationen,
    AdminPraesentationZeile? Gewaehlt,
    IReadOnlyList<ZuseherZeile> Zuseher);
