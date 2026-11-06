using MaturaArbeiten.Domain;

namespace MaturaArbeiten.ViewModels;

public sealed record ImportErgebnisViewModel(
    string Titel,
    int Geladen,
    IReadOnlyList<CsvFehler> Fehler);