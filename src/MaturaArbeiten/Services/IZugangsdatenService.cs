namespace MaturaArbeiten.Services;

public interface IZugangsdatenService
{
    (int Gesendet, int Fehlgeschlagen) Versenden();

    bool AnG3SuS(int g3susId);

    bool AnAdministrationslogin(int id);
}
