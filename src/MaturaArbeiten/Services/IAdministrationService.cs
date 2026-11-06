using MaturaArbeiten.ViewModels;

namespace MaturaArbeiten.Services;

public interface IAdministrationService
{
    UebersichtViewModel Uebersicht();

    bool PhaseUmschalten();

    ZuruecksetzenViewModel Vorschau();

    void Zuruecksetzen();
}
