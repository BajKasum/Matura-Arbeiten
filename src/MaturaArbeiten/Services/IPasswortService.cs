namespace MaturaArbeiten.Services;

public interface IPasswortService
{
    string? Aendern(string rolle, int id, string aktuell, string neu);
}
