using System.ComponentModel.DataAnnotations;

namespace MaturaArbeiten.ViewModels;

public sealed class G3SuSFormular
{
    [Required(ErrorMessage = "Bitte gib den Vornamen ein.")]
    [StringLength(50, ErrorMessage = "Der Vorname darf höchstens 50 Zeichen lang sein.")]
    public string Vorname { get; init; } = string.Empty;

    [Required(ErrorMessage = "Bitte gib den Nachnamen ein.")]
    [StringLength(50, ErrorMessage = "Der Nachname darf höchstens 50 Zeichen lang sein.")]
    public string Nachname { get; init; } = string.Empty;

    [Required(ErrorMessage = "Bitte gib die E-Mail-Adresse ein.")]
    [StringLength(100, ErrorMessage = "Die E-Mail-Adresse darf höchstens 100 Zeichen lang sein.")]
    [EmailAddress(ErrorMessage = "Die E-Mail-Adresse ist nicht gültig.")]
    public string Email { get; init; } = string.Empty;
}
