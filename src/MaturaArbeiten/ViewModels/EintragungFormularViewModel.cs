namespace MaturaArbeiten.ViewModels;

public sealed record EintragungFormularViewModel(
    EintragungFormular Eingabe,
    IReadOnlyList<Auswahl> G3SuS,
    IReadOnlyList<Auswahl> Praesentationen,
    string? G3SuSName);
