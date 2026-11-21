using MaturaArbeiten.Domain;
using MaturaArbeiten.ViewModels;

namespace MaturaArbeiten.Services;

public interface IListenService
{
    RaumuebersichtViewModel Raumuebersicht();

    string RaumuebersichtCsv();

    ZuseherlisteViewModel? Zuseherliste(int? praesentationId);

    CsvDatei? ZuseherlisteCsv(int praesentationId);

    IReadOnlyList<ZuseherBlatt> Zuseherlisten();

    string ZuseherlistenCsv();
}
