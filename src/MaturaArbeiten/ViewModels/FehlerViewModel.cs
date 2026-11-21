namespace MaturaArbeiten.ViewModels;

public class FehlerViewModel
{
    public string? AnfrageId { get; set; }

    public bool HatAnfrageId => !string.IsNullOrEmpty(AnfrageId);
}
