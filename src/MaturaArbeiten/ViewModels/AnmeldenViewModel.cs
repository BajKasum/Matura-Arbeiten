using System.ComponentModel.DataAnnotations;

namespace MaturaArbeiten.ViewModels;

public sealed class AnmeldenViewModel
{
    [Required(ErrorMessage = "Bitte gib deine E-Mail-Adresse ein.")]
    [StringLength(100, ErrorMessage = "Die E-Mail-Adresse darf höchstens 100 Zeichen lang sein.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Bitte gib dein Passwort ein.")]
    [StringLength(100, ErrorMessage = "Das Passwort darf höchstens 100 Zeichen lang sein.")]
    public string Passwort { get; init; } = string.Empty;
}