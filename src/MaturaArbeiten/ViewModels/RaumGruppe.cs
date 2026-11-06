namespace MaturaArbeiten.ViewModels;

public sealed record RaumGruppe(string Raum, IReadOnlyList<RaumPraesentation> Praesentationen);
