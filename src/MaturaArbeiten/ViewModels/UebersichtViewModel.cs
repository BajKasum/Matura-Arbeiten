namespace MaturaArbeiten.ViewModels;

public sealed record UebersichtViewModel(
    bool PhaseOffen,
    int GesamtG3SuS,
    int MitPflicht,
    int Pflicht,
    int ZugangsdatenVersendet,
    int BestaetigungenVersendet,
    int Fehlgeschlagen,
    IReadOnlyList<string> OhneMoeglichkeit)
{
    public bool AlleFertig => GesamtG3SuS > 0 && MitPflicht == GesamtG3SuS;
}
