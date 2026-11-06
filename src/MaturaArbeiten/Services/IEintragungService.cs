using MaturaArbeiten.Domain;
using MaturaArbeiten.ViewModels;

namespace MaturaArbeiten.Services;

public interface IEintragungService
{
    EintragungErgebnis Eintragen(int g3susId, int praesentationId);

    MeineEintragungenViewModel Meine(int g3susId);
}