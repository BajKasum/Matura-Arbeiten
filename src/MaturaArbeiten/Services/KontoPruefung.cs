using System.Security.Claims;
using MaturaArbeiten.Data;
using MaturaArbeiten.Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace MaturaArbeiten.Services;

public static class KontoPruefung
{
    public static async Task Pruefen(CookieValidatePrincipalContext kontext)
    {
        var db = kontext.HttpContext.RequestServices.GetRequiredService<AppDbContext>();

        var rolle = kontext.Principal?.FindFirstValue(ClaimTypes.Role);
        var id = kontext.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(id, out var kontoId) || !GibtEsOderUnklar(db, rolle, kontoId))
        {
            kontext.RejectPrincipal();
            await kontext.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }

    private static bool GibtEsOderUnklar(AppDbContext db, string? rolle, int id)
    {
        try
        {
            return GibtEs(db, rolle, id);
        }
        catch (Exception)
        {
            return true;
        }
    }

    private static bool GibtEs(AppDbContext db, string? rolle, int id) => rolle switch
    {
        Rollen.Administration => db.Administrationslogins.Any(a => a.Id == id),
        Rollen.G3SuS => db.G3SuSListe.Any(g => g.Id == id),
        _ => false
    };
}
