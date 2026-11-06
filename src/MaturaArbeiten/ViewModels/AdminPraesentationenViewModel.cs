namespace MaturaArbeiten.ViewModels;

public sealed record AdminPraesentationenViewModel(
    IReadOnlyList<AdminPraesentationZeile> Zeilen,
    int Platzkontingent);
