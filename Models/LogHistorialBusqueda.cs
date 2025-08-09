using System;

namespace BookRadar.Web.Models
{
    public class LogHistorialBusqueda
    {
        public long Id { get; set; }                 // id (BIGINT IDENTITY)
        public string Mensaje { get; set; } = default!; // NVARCHAR(MAX) con JSON
        public DateTime FechaCreacion { get; set; }  // datetime2(0) default SYSUTCDATETIME()
    }
}
