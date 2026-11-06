using MaturaArbeiten.ViewModels;

namespace MaturaArbeiten.Services;

public interface IEmailProtokollService
{
    EmailProtokollViewModel Fehlgeschlagene();

    bool ErneutSenden(int protokollId);

    (int Gesendet, int Fehlgeschlagen) AlleErneutSenden();
}
