namespace MaturaArbeiten.Models;
    public class EmailVersand
    {
    public int Id { get; set; }
    public int G3susId { get; set; }
    public string Typ { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime GesendetAm { get; set; }
    }

