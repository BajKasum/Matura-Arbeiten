using MaturaArbeiten.Domain;
using MaturaArbeiten.ViewModels;

namespace MaturaArbeiten.Services;

public interface IEintragungService
{
    EintragungErgebnis Eintragen(int g3susId, int praesentationId);

    EintragungErgebnis EintragenDurchAdministration(int g3susId, int praesentationId);

    EintragungErgebnis? Aendern(int id, int neuePraesentationId);

    bool Loeschen(int id);

    MeineEintragungenViewModel Meine(int g3susId);

    AdminEintragungenViewModel Alle();

    EintragungFormularViewModel? Formular(int? id);
}
