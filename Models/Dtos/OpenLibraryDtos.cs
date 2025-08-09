
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

        
        [JsonPropertyName("cover_edition_key")]
        public string? CoverEditionKey { get; set; }

        [JsonPropertyName("edition_key")]
        public List<string>? EditionKey { get; set; }
    }

    
    public class OpenLibraryEdition
    {
        [JsonPropertyName("publishers")]
        public List<string>? Publishers { get; set; }

        [JsonPropertyName("publish_date")]
        public string? PublishDate { get; set; }
    }
}
