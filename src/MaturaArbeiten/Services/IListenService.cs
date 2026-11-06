using MaturaArbeiten.ViewModels;

namespace MaturaArbeiten.Services;

public interface IListenService
{
    RaumuebersichtViewModel Raumuebersicht();

    string RaumuebersichtCsv();

    ZuseherlisteViewModel? Zuseherliste(int? praesentationId);

    string? ZuseherlisteCsv(int praesentationId);
}
