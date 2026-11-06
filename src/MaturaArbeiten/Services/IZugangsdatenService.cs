namespace MaturaArbeiten.Services;

public interface IZugangsdatenService
{
    (int Gesendet, int Fehlgeschlagen) Versenden();

    bool AnAdministrationslogin(int id);
}
