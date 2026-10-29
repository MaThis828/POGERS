using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pogers.Core.Entities;
using Pogers.Core.Services;
using Pogers.Core.WorkItems;
using Pogers.Infrastructure.Dip;

namespace Pogers.Infrastructure.Services
{
    /// <summary>
    /// Importiert die Personen einer Wahlperiode aus der DIP-API
    /// </summary>
    public class PersonImportService : IPersonImportService
    {
        private readonly IDipApiClient _client;
        private readonly PogersDbContext _db;
        private readonly ILogger<PersonImportService> _logger;

        public PersonImportService(
            IDipApiClient client, PogersDbContext db, ILogger<PersonImportService> logger)
        {
            _client = client;
            _db = db;
            _logger = logger;
        }

        public async Task<ImportBilanz> ImportAsync(int wahlperiode, CancellationToken ct = default)
        {
            // Einmal laden statt je Person abfragen
            var fraktionen = await _db.Fraktionen.ToDictionaryAsync(f => f.Name, ct);
            var personen = await _db.Personen
                .Include(p => p.Wahlperioden.Where(pw => pw.Wahlperiode == wahlperiode))
                .ToDictionaryAsync(p => p.Id, ct);

            var angelegt = 0;
            var aktualisiert = 0;
            var uebersprungen = 0;
            var ohneFraktion = 0;

            await foreach (var dto in _client.GetPersonenAsync(wahlperiode, ct))
            {
                if (!int.TryParse(dto.Id, out var id))
                {
                    _logger.LogWarning("Person mit unlesbarer Id {Id} uebersprungen.", dto.Id);
                    uebersprungen++;
                    continue;
                }

                if (!personen.TryGetValue(id, out var person))
                {
                    person = new Person { Id = id };
                    _db.Personen.Add(person);
                    personen[id] = person;
                    angelegt++;
                }
                else
                {
                    aktualisiert++;
                }

                person.Vorname = dto.Vorname;
                person.Nachname = dto.Nachname;
                person.Titel = LiesAkademischenTitel(dto);

                var mitgliedschaft = person.Wahlperioden.SingleOrDefault(pw => pw.Wahlperiode == wahlperiode);
                if (mitgliedschaft is null)
                {
                    mitgliedschaft = new PersonWahlperiode { Wahlperiode = wahlperiode };
                    person.Wahlperioden.Add(mitgliedschaft);
                }

                // Ohne Angabe bleibt eine bekannte Fraktion stehen, z. B. aus den Abstimmungslisten
                var fraktionsname = LiesFraktion(dto, wahlperiode);
                if (fraktionsname is not null)
                    mitgliedschaft.Fraktion = HoleOderLegeAn(fraktionsname, fraktionen);
                else
                    ohneFraktion++;
            }

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Personenimport Wahlperiode {Wahlperiode}: {Angelegt} angelegt, {Aktualisiert} aktualisiert, {OhneFraktion} ohne Fraktion, {Uebersprungen} uebersprungen.",
                wahlperiode, angelegt, aktualisiert, ohneFraktion, uebersprungen);

            return new ImportBilanz(angelegt, aktualisiert);
        }

        // DIP liefert "Dr. Michael Blos, MdB, AfD", gebraucht wird nur "Dr."
        public static string? LiesAkademischenTitel(PersonDto dto)
        {
            var ende = dto.Titel?.IndexOf(dto.Vorname, StringComparison.Ordinal) ?? -1;
            if (ende <= 0)
                return null;

            var titel = dto.Titel![..ende].Trim();
            return titel.Length == 0 ? null : titel;
        }

        // Bei Regierungsmitgliedern ist "fraktion" leer, die Fraktion steht dann in den Rollen
        public static string? LiesFraktion(PersonDto dto, int wahlperiode)
        {
            var aktuell = dto.Fraktion?.FirstOrDefault(f => !string.IsNullOrWhiteSpace(f));
            if (aktuell is not null)
                return aktuell;

            return dto.Rollen?
                .Where(r => r.Wahlperioden?.Contains(wahlperiode) == true)
                .Select(r => r.Fraktion)
                .FirstOrDefault(f => !string.IsNullOrWhiteSpace(f));
        }

        private Fraktion HoleOderLegeAn(string name, Dictionary<string, Fraktion> cache)
        {
            if (cache.TryGetValue(name, out var vorhanden))
                return vorhanden;

            var neu = new Fraktion { Name = name };
            _db.Fraktionen.Add(neu);
            cache[name] = neu;
            return neu;
        }
    }
}
