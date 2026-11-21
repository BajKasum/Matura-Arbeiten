namespace MaturaArbeiten.Domain;

public static class Regeln
{
    public const int PflichtEintragungen = 2;

    public static int FreiePlaetze(int platzkontingent, int belegtePlaetze) =>
        platzkontingent - belegtePlaetze;

    public static bool Kollidiert(Zeitpunkt a, Zeitpunkt b) => a == b;

    // Präsentationen zur gleichen Zeit zählen nur einmal, weil man nicht beide wählen darf.
    public static bool NochErreichbar(
        IReadOnlyCollection<Zeitpunkt> eigeneZeitpunkte,
        IEnumerable<Zeitpunkt> waehlbareZeitpunkte) =>
        eigeneZeitpunkte.Count >= PflichtEintragungen
        || waehlbareZeitpunkte.Distinct().Count() >= PflichtEintragungen - eigeneZeitpunkte.Count;

    // Feste Reihenfolge: Phase, zwei Eintragungen, freier Platz, Zeitkollision. Der erste Grund gewinnt.
    public static EintragungErgebnis Pruefe(
        bool phaseOffen,
        int platzkontingent,
        Zeitpunkt wunsch,
        int belegtePlaetze,
        IReadOnlyCollection<Zeitpunkt> eigeneZeitpunkte) => true switch
    {
        _ when !phaseOffen
            => new EintragungErgebnis.Abgewiesen(Abweisung.PhaseGeschlossen),
        _ when eigeneZeitpunkte.Count >= PflichtEintragungen
            => new EintragungErgebnis.Abgewiesen(Abweisung.SchonZweiEintragungen),
        _ when FreiePlaetze(platzkontingent, belegtePlaetze) <= 0
            => new EintragungErgebnis.Abgewiesen(Abweisung.PraesentationVoll),
        _ when eigeneZeitpunkte.Any(t => Kollidiert(t, wunsch))
            => new EintragungErgebnis.Abgewiesen(Abweisung.Zeitkollision),
        _ => new EintragungErgebnis.Erfolg()
    };
}
