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

        public IAsyncEnumerable<PersonDto> GetPersonenAsync(int wahlperiode, CancellationToken ct = default) =>
            HoleAlleSeitenAsync<PersonDto>("person", wahlperiode, ct);

        public IAsyncEnumerable<VorgangDto> GetVorgaengeAsync(int wahlperiode, CancellationToken ct = default) =>
            HoleAlleSeitenAsync<VorgangDto>("vorgang", wahlperiode, ct);

        private async IAsyncEnumerable<T> HoleAlleSeitenAsync<T>(
            string pfad, int wahlperiode, [EnumeratorCancellation] CancellationToken ct)
        {
            string? cursor = null;

            while (true)
            {
                var url = $"{_optionen.BaseUrl}/{pfad}?f.wahlperiode={wahlperiode}" +
                          (cursor is null ? "" : $"&cursor={Uri.EscapeDataString(cursor)}");

                // Key im Header, damit er nicht in URL-Logs landet
                using var anfrage = new HttpRequestMessage(HttpMethod.Get, url);
                anfrage.Headers.Authorization = new AuthenticationHeaderValue("ApiKey", _optionen.ApiKey);

                using var antwort = await _http.SendAsync(anfrage, ct);
                WirfBeiFehler(antwort);

                var seite = await antwort.Content
                    .ReadFromJsonAsync<DipPageResponse<T>>(cancellationToken: ct)
                    ?? throw new DipApiException("Die DIP-API lieferte eine leere Antwort.");

                foreach (var eintrag in seite.Documents)
                    yield return eintrag;

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
