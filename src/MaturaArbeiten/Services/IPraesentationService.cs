using MaturaArbeiten.ViewModels;

namespace MaturaArbeiten.Services;

public interface IPraesentationService
{
    PraesentationenViewModel Liste(int g3susId);
}