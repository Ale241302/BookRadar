// Models/Dtos/OpenLibraryDtos.cs
using System.Text.Json.Serialization;

namespace BookRadar.Web.Models.Dtos
{
    public class OpenLibraryResponse
    {
        [JsonPropertyName("docs")]
        public List<OpenLibraryDoc> Docs { get; set; } = new();
    }

    public class OpenLibraryDoc
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("first_publish_year")]
        public int? FirstPublishYear { get; set; }

        [JsonPropertyName("publisher")]
        public List<string>? Publisher { get; set; }

        [JsonPropertyName("publisher_facet")]
        public List<string>? PublisherFacet { get; set; }
    }
}
