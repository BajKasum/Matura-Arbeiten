namespace MaturaArbeiten.ViewModels;

public sealed record FehlgeschlagenZeile(
    int Id,
    string G3SuS,
    string Typ,
    DateTime Zeit);
