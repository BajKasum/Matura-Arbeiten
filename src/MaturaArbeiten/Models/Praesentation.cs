using MaturaArbeiten.Domain;

namespace MaturaArbeiten.Models;

public class Praesentation
{
    public int Id { get; set; }
    public string Thema { get; set; } = string.Empty;
    public DateOnly Datum { get; set; }
    public TimeOnly Uhrzeit { get; set; }
    public string Raum { get; set; } = string.Empty;
    public List<Eintragung> Eintragungen { get; set; } = [];

    public Zeitpunkt Zeitpunkt => new(Datum, Uhrzeit);

    public int GetFreiePlaetze(int platzkontingent) =>
        Math.Max(0, Regeln.FreiePlaetze(platzkontingent, Eintragungen.Count));

    public bool IstVoll(int platzkontingent) => GetFreiePlaetze(platzkontingent) <= 0;
}