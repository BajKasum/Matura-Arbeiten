namespace MaturaArbeiten.ViewModels;

public sealed record G3SuSZeile(
    int Id,
    string Vorname,
    string Nachname,
    string Email,
    int Eintragungen,
    bool HatPasswort);
