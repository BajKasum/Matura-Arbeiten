using MaturaArbeiten.Domain;

namespace MaturaArbeiten.Services;

public interface IBestaetigungService
{
    Versand Senden(int g3susId);

    (int Gesendet, int Fehlgeschlagen) AnAlle();
}
