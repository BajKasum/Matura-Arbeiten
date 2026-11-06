namespace MaturaArbeiten.ViewModels;

public sealed record PraesentationenViewModel(
    IReadOnlyList<PraesentationZeile> Zeilen,
    int GewaehlteAnzahl,
    int Pflicht,
    bool PhaseOffen,
    bool NochErreichbar);
