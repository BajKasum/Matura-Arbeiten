namespace MaturaArbeiten.Domain;

public static class Regeln
{
    public const int PflichtEintragungen = 2;

    public static int FreiePlaetze(int platzkontingent, int belegtePlaetze) =>
        platzkontingent - belegtePlaetze;

    public static bool Kollidiert(Termin a, Termin b) => a == b;

    public static EintragungErgebnis Pruefe(
        bool phaseOffen,
        int platzkontingent,
        Termin wunsch,
        int belegtePlaetze,
        IReadOnlyCollection<Termin> eigeneTermine) => true switch
        {
            _ when !phaseOffen
                => new EintragungErgebnis.Abgewiesen(Abweisung.PhaseGeschlossen),
            _ when eigeneTermine.Count >= PflichtEintragungen
                => new EintragungErgebnis.Abgewiesen(Abweisung.SchonZweiEintragungen),
            _ when FreiePlaetze(platzkontingent, belegtePlaetze) <= 0
                => new EintragungErgebnis.Abgewiesen(Abweisung.PraesentationVoll),
            _ when eigeneTermine.Any(t => Kollidiert(t, wunsch))
                => new EintragungErgebnis.Abgewiesen(Abweisung.Terminkollision),
            _ => new EintragungErgebnis.Erfolg()
        };
}