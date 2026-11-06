namespace MaturaArbeiten.Domain;

public sealed record ProtokollEintrag(int Id, int G3susId, string Typ, string Status, DateTime Zeit);
