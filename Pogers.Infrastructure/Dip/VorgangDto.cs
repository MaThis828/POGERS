using System.Text.Json.Serialization;

namespace Pogers.Infrastructure.Dip
{
    /// <summary>
    /// Vorgang, wie ihn die DIP-API unter /vorgang liefert
    /// </summary>
    public record VorgangDto(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("titel")] string Titel,
        [property: JsonPropertyName("vorgangstyp")] string? Vorgangstyp,
        [property: JsonPropertyName("wahlperiode")] int? Wahlperiode,
        [property: JsonPropertyName("datum")] string? Datum,
        [property: JsonPropertyName("beratungsstand")] string? Beratungsstand);
}
