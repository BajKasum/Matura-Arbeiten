namespace MaturaArbeiten.ViewModels;

public sealed record AdminLoginZeile(
    int Id,
    string Vorname,
    string Nachname,
    string Email,
    bool HatPasswort);
