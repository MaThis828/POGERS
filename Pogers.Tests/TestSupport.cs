using System.Net;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Pogers.Infrastructure;
using Pogers.Infrastructure.Dip;

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

    /// <summary>
    /// Liefert vorbereitete Antworten der Reihe nach und merkt sich die Anfragen
    /// </summary>
    public sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _antworten = new();

        public List<string> AngefragteUrls { get; } = new();
        public List<string?> AuthorizationHeader { get; } = new();

        public FakeHttpMessageHandler RespondWithJson(string json)
        {
            _antworten.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
            return this;
        }

        public FakeHttpMessageHandler RespondWith(HttpStatusCode status)
        {
            _antworten.Enqueue(new HttpResponseMessage(status));
            return this;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage anfrage, CancellationToken cancellationToken)
        {
            AngefragteUrls.Add(anfrage.RequestUri!.ToString());
            AuthorizationHeader.Add(anfrage.Headers.Authorization?.ToString());

            if (_antworten.Count == 0)
                throw new InvalidOperationException($"Keine Antwort vorbereitet fuer {anfrage.RequestUri}.");

            return Task.FromResult(_antworten.Dequeue());
        }

        public HttpClient CreateClient() => new(this);
    }

    /// <summary>
    /// Testdaten im Format der DIP-API
    /// </summary>
    public static class TestFactory
    {
        public static DipApiClient DipClient(HttpClient http) =>
            new(http, Options.Create(new DipApiOptions
            {
                BaseUrl = "https://dip.test/api/v1",
                ApiKey = "testkey"
            }));

        public static ILogger<T> Logger<T>() => NullLogger<T>.Instance;

        public static string Seite(string? cursor, params string[] dokumenteJson) =>
            $$"""
            {
              "numFound": {{dokumenteJson.Length}},
              "documents": [{{string.Join(",", dokumenteJson)}}],
              "cursor": {{(cursor is null ? "null" : $"\"{cursor}\"")}}
            }
            """;

        public static string Person(int id, string vorname, string nachname,
            string? fraktion = "SPD", string? titel = null) =>
            $$"""
            {
              "id": "{{id}}",
              "vorname": "{{vorname}}",
              "nachname": "{{nachname}}",
              "titel": {{(titel is null ? "null" : $"\"{titel}\"")}},
              "fraktion": {{(fraktion is null ? "null" : $"[\"{fraktion}\"]")}},
              "wahlperiode": [21]
            }
            """;

        public static string Vorgang(int id, string titel, string? vorgangstyp = "Gesetzgebung",
            string? beratungsstand = "Noch nicht beraten") =>
            $$"""
            {
              "id": "{{id}}",
              "titel": "{{titel}}",
              "vorgangstyp": {{(vorgangstyp is null ? "null" : $"\"{vorgangstyp}\"")}},
              "wahlperiode": 21,
              "datum": "2026-07-10",
              "beratungsstand": {{(beratungsstand is null ? "null" : $"\"{beratungsstand}\"")}}
            }
            """;

        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> quelle)
        {
            var liste = new List<T>();
            await foreach (var element in quelle)
                liste.Add(element);
            return liste;
        }
    }
}
