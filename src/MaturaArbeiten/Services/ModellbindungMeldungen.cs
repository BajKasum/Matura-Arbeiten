using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace MaturaArbeiten.Services;

public static class ModellbindungMeldungen
{
    private const string Ungueltig = "Bitte wähle einen gültigen Eintrag aus.";

    public static void Setzen(DefaultModelBindingMessageProvider meldungen)
    {
        meldungen.SetValueMustNotBeNullAccessor(_ => "Bitte fülle dieses Feld aus.");
        meldungen.SetMissingBindRequiredValueAccessor(feld => $"Das Feld {feld} fehlt.");
        meldungen.SetMissingKeyOrValueAccessor(() => "Ein Wert fehlt.");
        meldungen.SetMissingRequestBodyRequiredValueAccessor(() => "Der Inhalt der Anfrage fehlt.");
        meldungen.SetValueIsInvalidAccessor(_ => Ungueltig);
        meldungen.SetAttemptedValueIsInvalidAccessor((_, _) => Ungueltig);
        meldungen.SetUnknownValueIsInvalidAccessor(_ => Ungueltig);
        meldungen.SetNonPropertyAttemptedValueIsInvalidAccessor(_ => Ungueltig);
        meldungen.SetNonPropertyUnknownValueIsInvalidAccessor(() => Ungueltig);
        meldungen.SetValueMustBeANumberAccessor(_ => "Bitte gib eine Zahl ein.");
        meldungen.SetNonPropertyValueMustBeANumberAccessor(() => "Bitte gib eine Zahl ein.");
    }
}
