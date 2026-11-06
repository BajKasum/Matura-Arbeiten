using MaturaArbeiten.ViewModels;

namespace MaturaArbeiten.Services;

public interface IG3SuSVerwaltungService
{
    IReadOnlyList<G3SuSZeile> Liste();

    G3SuSFormular? Finden(int id);

    string? Anlegen(G3SuSFormular eingabe);

    string? Aendern(int id, G3SuSFormular eingabe);

    bool Loeschen(int id);
}
