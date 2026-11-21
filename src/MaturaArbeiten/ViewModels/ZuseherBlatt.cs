namespace MaturaArbeiten.ViewModels;

public sealed record ZuseherBlatt(
    AdminPraesentationZeile Praesentation,
    IReadOnlyList<ZuseherZeile> Zuseher);
