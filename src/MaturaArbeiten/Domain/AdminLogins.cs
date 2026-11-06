namespace MaturaArbeiten.Domain;

public static class AdminLogins
{
    public static bool DarfGeloeschtWerden(int anzahlLogins) => anzahlLogins > 1;
}
