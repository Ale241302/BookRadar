namespace BookRadar.Web.Models.ViewModels
{
    public record BookResult(string Titulo, int? Anio, string? Editorial);

    public class SearchBooksViewModel
    {
        public string? Autor { get; set; }
        public List<BookResult> Resultados { get; set; } = new();
    }
}
