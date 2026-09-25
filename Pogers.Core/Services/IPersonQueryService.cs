using Pogers.Core.WorkItems;
namespace Pogers.Core.Services
{
    /// <summary>
    /// Zum Lesen der Personen Daten
    /// </summary>
    public interface IPersonQueryService
    {
        Task<IReadOnlyList<PersonListItem>> GetPersonenAsync(
            string? fraktion, CancellationToken ct = default);
    }
}