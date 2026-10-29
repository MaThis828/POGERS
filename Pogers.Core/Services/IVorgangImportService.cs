using Pogers.Core.WorkItems;
namespace Pogers.Core.Services
{
    /// <summary>
    /// Importiert die Vorgaenge (Gesetzentwuerfe, Antraege, Anfragen) einer Wahlperiode
    /// </summary>
    public interface IVorgangImportService
    {
        Task<ImportBilanz> ImportAsync(int wahlperiode, CancellationToken ct = default);
    }
}
