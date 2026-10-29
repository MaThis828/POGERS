using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Pogers.Infrastructure;

namespace Pogers.Tests
{
    /// <summary>
    /// Echte SQLite-Datenbank im Arbeitsspeicher, aufgebaut ueber die Migrationen
    /// wie in der Produktion. Der InMemory-Provider wuerde Unique-Indizes und
    /// Fremdschluessel nicht pruefen.
    /// </summary>
    public sealed class TestDb : IDisposable
    {
        private readonly SqliteConnection _verbindung;
        private readonly DbContextOptions<PogersDbContext> _optionen;

        public TestDb()
        {
            _verbindung = new SqliteConnection("DataSource=:memory:");
            _verbindung.Open();

            _optionen = new DbContextOptionsBuilder<PogersDbContext>()
                .UseSqlite(_verbindung)
                .Options;

            using var ctx = new PogersDbContext(_optionen);
            ctx.Database.Migrate();
        }

        /// <summary>
        /// Frischer Context ohne geerbten Change-Tracker
        /// </summary>
        public PogersDbContext NewContext() => new(_optionen);

        public void Dispose() => _verbindung.Dispose();
    }
}
