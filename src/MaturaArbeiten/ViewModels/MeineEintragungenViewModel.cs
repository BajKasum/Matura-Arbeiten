namespace MaturaArbeiten.ViewModels;

public sealed record MeineEintragungenViewModel(
    IReadOnlyList<EintragungZeile> Zeilen,
    int Pflicht)
{
    public bool PflichtErfuellt => Zeilen.Count >= Pflicht;
}