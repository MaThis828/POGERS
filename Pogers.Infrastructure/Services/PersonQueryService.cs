using Microsoft.EntityFrameworkCore;
using Pogers.Core.Services;
using Pogers.Core.WorkItems;

namespace Pogers.Infrastructure.Services
{
    /// <summary>
    /// Liest die Abgeordneten einer Wahlperiode, optional gefiltert nach Fraktion
    /// </summary>
    public class PersonQueryService : IPersonQueryService
    {
        private readonly PogersDbContext _db;

        public PersonQueryService(PogersDbContext db) => _db = db;

        public async Task<IReadOnlyList<PersonListItem>> GetPersonenAsync(
            int wahlperiode, string? fraktion, CancellationToken ct = default)
        {
            // Ausgangspunkt ist die Mitgliedschaft, dort haengt die Fraktion je Wahlperiode
            // Nur mit Fraktion: ohne sind es Bundesratsmitglieder oder Regierungsrollen ohne Mandat
            var abfrage = _db.PersonWahlperioden
                .AsNoTracking()
                .Where(pw => pw.Wahlperiode == wahlperiode && pw.Fraktion != null);

            if (!string.IsNullOrWhiteSpace(fraktion))
                abfrage = abfrage.Where(pw => pw.Fraktion!.Name == fraktion);

            return await abfrage
                .OrderBy(pw => pw.Person.Nachname)
                .ThenBy(pw => pw.Person.Vorname)
                .Select(pw => new PersonListItem(
                    pw.Person.Id,
                    pw.Person.Vorname,
                    pw.Person.Nachname,
                    pw.Person.Titel,
                    pw.Fraktion!.Name))
                .ToListAsync(ct);
        }
    }
}
