using MaturaArbeiten.Domain;

namespace MaturaArbeiten.Services;

public interface IAnmeldeService
{
    AnmeldeErgebnis Anmelden(string email, string passwort);
}