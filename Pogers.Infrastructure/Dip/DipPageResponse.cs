using System.Text.Json.Serialization;

namespace Pogers.Infrastructure.Dip
{
    /// <summary>
    /// Eine Ergebnisseite der DIP-API
    /// </summary>
    public record DipPageResponse<T>(
        [property: JsonPropertyName("numFound")] int NumFound,
        [property: JsonPropertyName("documents")] List<T> Documents,
        [property: JsonPropertyName("cursor")] string? Cursor);
}
