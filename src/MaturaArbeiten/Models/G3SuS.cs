namespace MaturaArbeiten.Models;

public class G3SuS
{
    public int Id { get; set; }
    public string Vorname { get; set; } = string.Empty;
    public string Nachname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PasswortHash { get; set; }
    public bool PasswortGeaendert { get; set; }
    public int Fehlversuche { get; set; }
    public DateTime? GesperrtBis { get; set; }
}