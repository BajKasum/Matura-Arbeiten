using System.ComponentModel.DataAnnotations;

namespace MaturaArbeiten.ViewModels;

public sealed class EintragungFormular
{
    [Range(1, int.MaxValue, ErrorMessage = "Bitte wähle einen G3-SuS.")]
    public int G3susId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Bitte wähle eine Präsentation.")]
    public int PraesentationId { get; init; }
}
