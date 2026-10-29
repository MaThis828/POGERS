namespace Pogers.Infrastructure.Dip
{
    /// <summary>
    /// Zugriff auf die DIP-API. Liegt in Infrastructure, damit Core kein HTTP und JSON kennt.
    /// </summary>
    public interface IDipApiClient
    {
        IAsyncEnumerable<PersonDto> GetPersonenAsync(int wahlperiode, CancellationToken ct = default);

        IAsyncEnumerable<VorgangDto> GetVorgaengeAsync(int wahlperiode, CancellationToken ct = default);
    }
}
