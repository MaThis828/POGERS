using Pogers.Core.WorkItems;
namespace Pogers.Core.Services
{
    /// <summary>
    /// Import Personen nach Wahlperiode und hält sie im Cache 
    /// </summary>
    public interface IPersonImportService
    {
        Task<ImportBilanz> ImportAsync(int wahlperiode, CancellationToken ct = default);
    }
}
