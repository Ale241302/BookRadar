using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BookRadar.Web.Data;
using BookRadar.Web.Models;
using BookRadar.Web.Models.ViewModels;
using BookRadar.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BookRadar.Web.Controllers
{
    public class BooksController : Controller
    {
        private readonly AppDbContext _db;
        private readonly OpenLibraryClient _client;
        private readonly IConfiguration _cfg;

        public BooksController(AppDbContext db, OpenLibraryClient client, IConfiguration cfg)
        {
            _db = db;
            _client = client;
            _cfg = cfg;
        }

        // GET: form vacío
        [HttpGet]
        public IActionResult Index() => View(new SearchBooksViewModel());

        // POST: buscar, mostrar resultados, guardar historial y loguear JSON crudo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SearchBooksViewModel vm, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(vm.Autor))
            {
                ModelState.AddModelError(nameof(vm.Autor), "Ingresa un autor.");
                return View(vm);
            }

            // Trae lista para la UI + JSON crudo para el log
            var (rawJson, results) = await _client.SearchByAuthorWithRawAsync(vm.Autor!.Trim(), ct);

            vm.Resultados = results
                .Select(r => new BookResult(r.Titulo, r.Anio, r.Editorial))
                .ToList();

            try
            {
                // 1) Guardar historial (TVP + SP con dedupe < 1 min)
                await GuardarResultadosAsync(vm.Autor!, results, ct);

                // 2) Log: JSON crudo de la API en dbo.log_HistorialBusquedas.mensaje
                await LogBusquedaApiAsync(vm.Autor!, rawJson, ct);
            }
            catch
            {
                
            }

            return View(vm);
        }

        // GET: historial desde BD
        [HttpGet]
        public async Task<IActionResult> Historial(CancellationToken ct)
        {
            var data = await _db.HistorialBusquedas
                .AsNoTracking()
                .OrderByDescending(x => x.FechaConsulta)
                .Take(500)
                .ToListAsync(ct);

            return View(data);
        }

        // Inserta filas usando TVP dbo.BookRow y SP dbo.usp_InsertarHistorialDesdeResultados
        private async Task GuardarResultadosAsync(string author, List<(string Titulo, int? Anio, string? Editorial)> rows, CancellationToken ct)
        {
            // DataTable para TVP dbo.BookRow
            var tvp = new DataTable();
            tvp.Columns.Add("Autor", typeof(string));
            tvp.Columns.Add("Titulo", typeof(string));
            tvp.Columns.Add("AnioPublicacion", typeof(int));
            tvp.Columns.Add("Editorial", typeof(string));

            foreach (var r in rows)
            {
                tvp.Rows.Add(
                    author,
                    r.Titulo,
                    r.Anio.HasValue ? r.Anio.Value : (object)DBNull.Value,
                    r.Editorial ?? (object)DBNull.Value
                );
            }

            var cs = _cfg.GetConnectionString("Default")!;
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync(ct);

            await using var cmd = new SqlCommand("dbo.usp_InsertarHistorialDesdeResultados", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            var p = cmd.Parameters.AddWithValue("@Resultados", tvp);
            p.SqlDbType = SqlDbType.Structured;
            p.TypeName = "dbo.BookRow";

            await cmd.ExecuteNonQueryAsync(ct);
        }

        // Llama al SP dbo.usp_LogBusquedaApi para guardar el JSON crudo en log_HistorialBusquedas
        private async Task LogBusquedaApiAsync(string author, string rawJson, CancellationToken ct)
        {
            var cs = _cfg.GetConnectionString("Default")!;
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync(ct);

            await using var cmd = new SqlCommand("dbo.usp_LogBusquedaApi", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@autor", author);
            cmd.Parameters.AddWithValue("@rawJson", rawJson);

            await cmd.ExecuteNonQueryAsync(ct);
        }
    }
}
