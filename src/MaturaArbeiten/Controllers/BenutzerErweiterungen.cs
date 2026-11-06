using System.Security.Claims;

namespace MaturaArbeiten.Controllers;

public static class BenutzerErweiterungen
{
    public static int BenutzerId(this ClaimsPrincipal benutzer) =>
        int.Parse(benutzer.FindFirstValue(ClaimTypes.NameIdentifier)!);
}