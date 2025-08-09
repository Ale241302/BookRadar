// Services/OpenLibraryClient.cs
using System.Net.Http.Json;
using System.Text.Json;
using BookRadar.Web.Models.Dtos;

namespace BookRadar.Web.Services
{
    public class OpenLibraryClient
    {
        private readonly HttpClient _http;

        public OpenLibraryClient(HttpClient http) => _http = http;

        public async Task<(string rawJson, List<(string Titulo, int? Anio, string? Editorial)> rows)>
            SearchByAuthorWithRawAsync(string author, CancellationToken ct = default)
        {
            var url = $"search.json?author={Uri.EscapeDataString(author)}";

            // 1) RAW JSON
            var raw = await _http.GetStringAsync(url, ct);

            // 2) Parse
            var resp = JsonSerializer.Deserialize<OpenLibraryResponse>(raw, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            static string? PickPublisher(OpenLibraryDoc d)
            {
                var p = d.Publisher?.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s))
                     ?? d.PublisherFacet?.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
                return p?.Trim();
            }

            var rows = (resp?.Docs ?? new())
                .Select(d => (d.Title ?? "(Sin título)", d.FirstPublishYear, PickPublisher(d)))
                .ToList();

            return (raw, rows);
        }
    }
}
