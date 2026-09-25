namespace MaturaArbeiten.Models;
    public class Administrationslogin
    {
    public Id { get; set; }
    public string Vorname { get; set; } = string.Empty;
    public string Nachname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty; //wert darf nicht null sein .Empty
    public string? PasswortHash { get; set; }
    public int Fehlversuche { get; set; }
    public DateTime? GesperrtBis { get; set; } //wert darf null sein ?
}

