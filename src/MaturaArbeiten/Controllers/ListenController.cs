using System.Text;
using MaturaArbeiten.Domain;
using MaturaArbeiten.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaturaArbeiten.Controllers;

[Authorize(Roles = Rollen.Administration)]
public class ListenController(IListenService service) : Controller
{
    public IActionResult Index() => View();

    public IActionResult Raumuebersicht() => View(service.Raumuebersicht());

    public IActionResult RaumuebersichtCsv() => Csv(service.RaumuebersichtCsv(), "Raumuebersicht.csv");

    public IActionResult Zuseherliste(int? id) =>
        service.Zuseherliste(id) is { } liste ? View(liste) : NotFound();

    public IActionResult ZuseherlisteCsv(int id) =>
        service.ZuseherlisteCsv(id) is { } datei ? Csv(datei.Inhalt, datei.Name) : NotFound();

    public IActionResult Zuseherlisten() => View(service.Zuseherlisten());

    public IActionResult ZuseherlistenCsv() => Csv(service.ZuseherlistenCsv(), "Zuseherlisten.csv");

    private FileContentResult Csv(string inhalt, string dateiname)
    {
        Response.Headers.CacheControl = "no-store";

        return File(
            Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(inhalt)).ToArray(),
            "text/csv; charset=utf-8",
            dateiname);
    }
}
