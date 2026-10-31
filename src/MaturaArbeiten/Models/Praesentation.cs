namespace MaturaArbeiten.Models;

public class Praesentation
{
    public int Id { get; set; }
    public string Thema { get; set; } = string.Empty;
    public DateOnly Datum { get; set; }
    public TimeOnly Uhrzeit { get; set; }
    public string Raum { get; set; } = string.Empty;
    public List<Eintragung> Eintragungen { get; set; } = [];
}
