using Pogers.Core.Services;

namespace Pogers.Worker
{
    /// <summary>
    /// Importiert einmal beim Start und beendet danach den Host
    /// </summary>
    public class ImportHostedService : BackgroundService
    {
        private readonly IServiceProvider _dienste;
        private readonly IHostApplicationLifetime _lebenszyklus;
        private readonly IConfiguration _konfiguration;
        private readonly ILogger<ImportHostedService> _logger;

        public ImportHostedService(
            IServiceProvider dienste,
            IHostApplicationLifetime lebenszyklus,
            IConfiguration konfiguration,
            ILogger<ImportHostedService> logger)
        {
            _dienste = dienste;
            _lebenszyklus = lebenszyklus;
            _konfiguration = konfiguration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var wahlperiode = _konfiguration.GetValue("Wahlperiode", 21);

            try
            {
                using var bereich = _dienste.CreateScope();
                var dienste = bereich.ServiceProvider;

                await dienste.GetRequiredService<IPersonImportService>().ImportAsync(wahlperiode, stoppingToken);
                await dienste.GetRequiredService<IVorgangImportService>().ImportAsync(wahlperiode, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Import abgebrochen.");
            }
            catch (Exception fehler)
            {
                _logger.LogError(fehler, "Der Import ist fehlgeschlagen.");
                Environment.ExitCode = 1;
            }
            finally
            {
                _lebenszyklus.StopApplication();
            }
        }
    }
}
