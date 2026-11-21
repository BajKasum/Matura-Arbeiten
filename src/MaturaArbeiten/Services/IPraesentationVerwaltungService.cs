using MaturaArbeiten.Domain;
using MaturaArbeiten.ViewModels;

namespace MaturaArbeiten.Services;

public interface IPraesentationVerwaltungService
{
    PraesentationFormular? Finden(int id);

    PraesentationErgebnis Anlegen(PraesentationFormular eingabe);

    PraesentationErgebnis Aendern(int id, PraesentationFormular eingabe);

    PraesentationErgebnis Loeschen(int id);
}
