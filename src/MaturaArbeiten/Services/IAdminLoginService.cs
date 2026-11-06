using MaturaArbeiten.ViewModels;

namespace MaturaArbeiten.Services;

public interface IAdminLoginService
{
    IReadOnlyList<AdminLoginZeile> Liste();

    PersonFormular? Finden(int id);

    (int? Id, string? Fehler) Anlegen(PersonFormular eingabe);

    string? Aendern(int id, PersonFormular eingabe);

    string? Loeschen(int id);
}
