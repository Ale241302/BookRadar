using System;
namespace BookRadar.Web.Models
{
    public class HistorialBusqueda
    {
        public long Id { get; set; }
        public string Autor { get; set; } = default!;
        public string Titulo { get; set; } = default!;
        public int? AnioPublicacion { get; set; }
        public string? Editorial { get; set; }
        public DateTime FechaConsulta { get; set; }
    }
}
