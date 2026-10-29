using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pogers.Core.Entities;
using Pogers.Core.Services;
using Pogers.Core.WorkItems;
using Pogers.Infrastructure.Dip;

namespace Pogers.Infrastructure.Services
{
    /// <summary>
    /// Importiert die Vorgaenge einer Wahlperiode aus der DIP-API
    /// </summary>
    public class VorgangImportService : IVorgangImportService
    {
        private readonly IDipApiClient _client;
        private readonly PogersDbContext _db;
        private readonly ILogger<VorgangImportService> _logger;

        public VorgangImportService(
            IDipApiClient client, PogersDbContext db, ILogger<VorgangImportService> logger)
        {
            _client = client;
            _db = db;
            _logger = logger;
        }

        public async Task<ImportBilanz> ImportAsync(int wahlperiode, CancellationToken ct = default)
        {
            // Rund 20.000 Vorgaenge je Wahlperiode: einmal laden statt je Vorgang abfragen.
            // Ohne Wahlperioden-Filter, sonst kollidiert ein Vorgang mit geaenderter Wahlperiode beim Anlegen.
            var vorgaenge = await _db.Vorgaenge.ToDictionaryAsync(v => v.Id, ct);

            var angelegt = 0;
            var aktualisiert = 0;

            await foreach (var dto in _client.GetVorgaengeAsync(wahlperiode, ct))
            {
                if (!int.TryParse(dto.Id, out var id))
                {
                    _logger.LogWarning("Vorgang mit unlesbarer Id {Id} uebersprungen.", dto.Id);
                    continue;
                }

                if (!vorgaenge.TryGetValue(id, out var vorgang))
                {
                    vorgang = new Vorgang { Id = id };
                    _db.Vorgaenge.Add(vorgang);
                    vorgaenge[id] = vorgang;
                    angelegt++;
                }
                else
                {
                    aktualisiert++;
                }

                vorgang.Titel = dto.Titel;
                vorgang.Vorgangstyp = dto.Vorgangstyp;
                vorgang.Wahlperiode = dto.Wahlperiode;
                vorgang.Datum = dto.Datum;
                vorgang.Beratungsstand = dto.Beratungsstand;
            }

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Vorgangsimport Wahlperiode {Wahlperiode}: {Angelegt} angelegt, {Aktualisiert} aktualisiert.",
                wahlperiode, angelegt, aktualisiert);

            return new ImportBilanz(angelegt, aktualisiert);
        }
    }
}
