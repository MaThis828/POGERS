using System.Text.Json.Serialization;

namespace Pogers.Infrastructure.Dip
{
    /// <summary>
    /// Person, wie sie die DIP-API unter /person liefert
    /// </summary>
    public record PersonDto(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("vorname")] string Vorname,
        [property: JsonPropertyName("nachname")] string Nachname,
        [property: JsonPropertyName("titel")] string? Titel,
        [property: JsonPropertyName("fraktion")] List<string>? Fraktion,
        [property: JsonPropertyName("wahlperiode")] List<int>? Wahlperiode,
        [property: JsonPropertyName("person_roles")] List<PersonRolleDto>? Rollen);

    /// <summary>
    /// Fruehere oder weitere Rolle einer Person, traegt die Fraktion je Wahlperiode
    /// </summary>
    public record PersonRolleDto(
        [property: JsonPropertyName("fraktion")] string? Fraktion,
        [property: JsonPropertyName("wahlperiode_nummer")] List<int>? Wahlperioden);
}
