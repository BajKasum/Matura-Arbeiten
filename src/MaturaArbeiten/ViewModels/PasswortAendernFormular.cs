using System.ComponentModel.DataAnnotations;

namespace MaturaArbeiten.ViewModels;

public sealed class PasswortAendernFormular
{
    [Required(ErrorMessage = "Bitte gib dein bisheriges Passwort ein.")]
    [StringLength(100, ErrorMessage = "Das Passwort darf höchstens 100 Zeichen lang sein.")]
    public string Aktuell { get; init; } = string.Empty;

    [Required(ErrorMessage = "Bitte gib ein neues Passwort ein.")]
    [StringLength(100, ErrorMessage = "Das Passwort darf höchstens 100 Zeichen lang sein.")]
    public string Neu { get; init; } = string.Empty;

    [Required(ErrorMessage = "Bitte wiederhole das neue Passwort.")]
    [StringLength(100, ErrorMessage = "Das Passwort darf höchstens 100 Zeichen lang sein.")]
    public string Wiederholung { get; init; } = string.Empty;
}
