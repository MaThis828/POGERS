using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;

namespace Pogers.Infrastructure.Dip
{
    /// <summary>
    /// Liest die DIP-API seitenweise ueber den Cursor
    /// </summary>
    public class DipApiClient : IDipApiClient
    {
        private readonly HttpClient _http;
        private readonly DipApiOptions _optionen;

        public DipApiClient(HttpClient http, IOptions<DipApiOptions> optionen)
        {
            _http = http;
            _optionen = optionen.Value;
        }

        public async IAsyncEnumerable<PersonDto> GetPersonenAsync(
            int wahlperiode, [EnumeratorCancellation] CancellationToken ct = default)
        {
            string? cursor = null;

            while (true)
            {
                var url = $"{_optionen.BaseUrl}/person?f.wahlperiode={wahlperiode}" +
                          (cursor is null ? "" : $"&cursor={Uri.EscapeDataString(cursor)}");

                // Key im Header, damit er nicht in URL-Logs landet
                using var anfrage = new HttpRequestMessage(HttpMethod.Get, url);
                anfrage.Headers.Authorization = new AuthenticationHeaderValue("ApiKey", _optionen.ApiKey);

                using var antwort = await _http.SendAsync(anfrage, ct);
                WirfBeiFehler(antwort);

                var seite = await antwort.Content
                    .ReadFromJsonAsync<DipPageResponse<PersonDto>>(cancellationToken: ct)
                    ?? throw new DipApiException("Die DIP-API lieferte eine leere Antwort.");

                foreach (var person in seite.Documents)
                    yield return person;

                // Auf der letzten Seite wiederholt die API den Cursor
                if (seite.Cursor is null || seite.Cursor == cursor || seite.Documents.Count == 0)
                    break;

                cursor = seite.Cursor;
            }
        }

        private static void WirfBeiFehler(HttpResponseMessage antwort)
        {
            if (antwort.IsSuccessStatusCode)
                return;

            throw new DipApiException(antwort.StatusCode switch
            {
                HttpStatusCode.Unauthorized =>
                    "Die DIP-API hat den API-Key abgelehnt (401). Pruefen mit: " +
                    "dotnet user-secrets list --project Pogers.Worker",
                HttpStatusCode.TooManyRequests =>
                    "Die DIP-API hat wegen zu vieler Anfragen abgelehnt (429). Spaeter erneut versuchen.",
                _ =>
                    $"Die DIP-API antwortete mit {(int)antwort.StatusCode} {antwort.ReasonPhrase}."
            });
        }
    }
}
