using MaturaArbeiten.ViewModels;

namespace MaturaArbeiten.Services;

public interface IG3SuSVerwaltungService
{
    IReadOnlyList<G3SuSZeile> Liste();

    PersonFormular? Finden(int id);

    string? Anlegen(PersonFormular eingabe);

    string? Aendern(int id, PersonFormular eingabe);

    bool Loeschen(int id);
}
