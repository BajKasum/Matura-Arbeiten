using MaturaArbeiten.ViewModels;

namespace MaturaArbeiten.Services;

public interface IImportService
{
    ImportErgebnisViewModel PraesentationenLaden(Stream datei);

    ImportErgebnisViewModel G3SuSLaden(Stream datei);
}