// Services/OpenLibraryClient.cs
using System.Net.Http.Json;
using System.Text.Json;
using BookRadar.Web.Models.Dtos;

namespace BookRadar.Web.Services
{
    public class OpenLibraryClient
    {
        private readonly HttpClient _http;   
        private readonly HttpClient _books; 

        
        public OpenLibraryClient(HttpClient http, IHttpClientFactory factory)
        {
            _http = http;
            _books = factory.CreateClient("OLBooks");
        }

        public async Task<(string rawJson, List<(string Titulo, int? Anio, string? Editorial)> rows)>
            SearchByAuthorWithRawAsync(string author, CancellationToken ct = default)
        {
            var url = $"search.json?author={Uri.EscapeDataString(author)}";

            var raw = await _http.GetStringAsync(url, ct);

            var resp = JsonSerializer.Deserialize<OpenLibraryResponse>(raw, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            static string? PickPublisherFromSearch(OpenLibraryDoc d)
            {
                var p = d.Publisher?.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s))
                     ?? d.PublisherFacet?.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
                return p?.Trim();
            }

            var docs = resp?.Docs ?? new();

            var prelim = docs.Select(d => new
            {
                Titulo = d.Title ?? "(Sin título)",
                Anio = d.FirstPublishYear,
                Editorial = PickPublisherFromSearch(d),
                EditionToQuery = d.CoverEditionKey
                                 ?? d.EditionKey?.FirstOrDefault()
            }).ToList();

            var needEdition = prelim
                .Where(x => string.IsNullOrWhiteSpace(x.Editorial) && !string.IsNullOrWhiteSpace(x.EditionToQuery))
                .ToList();

            if (needEdition.Count > 0)
            {
                var gate = new SemaphoreSlim(8);
                var tasks = needEdition.Select(async item =>
                {
                    await gate.WaitAsync(ct);
                    try
                    {
                        var edPublisher = await FetchPublisherFromEditionAsync(item.EditionToQuery!, ct);
                        var idx = prelim.FindIndex(p => ReferenceEquals(p, item));
                        if (idx >= 0 && !string.IsNullOrWhiteSpace(edPublisher))
                        {
                            prelim[idx] = new
                            {
                                item.Titulo,
                                item.Anio,
                                Editorial = edPublisher,
                                item.EditionToQuery
                            };
                        }
                    }
                    catch {  }
                    finally { gate.Release(); }
                }).ToArray();

                await Task.WhenAll(tasks);
            }

            var rows = prelim
                .Select(x => (x.Titulo, x.Anio, x.Editorial))
                .ToList();

            return (raw, rows);
        }

        private async Task<string?> FetchPublisherFromEditionAsync(string editionKey, CancellationToken ct)
        {
            
            var ed = await _books.GetFromJsonAsync<OpenLibraryEdition>($"{editionKey}.json", ct);
            return ed?.Publishers?.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p))?.Trim();
        }
    }
}
