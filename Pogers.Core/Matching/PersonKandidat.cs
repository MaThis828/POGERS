namespace Pogers.Core.Matching
{
    /// <summary>
    /// Eine Person mit ihrer Fraktion in der betrachteten Wahlperiode, soweit bekannt
    /// </summary>
    public record PersonKandidat(int PersonId, string Vorname, string Nachname, string? Fraktion);
}
