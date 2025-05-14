using DevExpressSqlserver.Infraestructure.Conexion;
using DevExpressSqlserver.Models.Entities;
using DevExpressSqlserver.Models.ModelViews;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace DevExpressSqlserver.Controllers
{
    [Authorize]
    public class FiltrosMovimientosController : Controller
    {
        private readonly SBERPDbContext _context;

        public FiltrosMovimientosController(SBERPDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); 
        }
        public IActionResult Index()
        {
            return View();
        }

        // GET: Movimientos/Filtrar
        [HttpGet]
        public IActionResult Get(DataSourceLoadOptions loadOptions, string values = null)
        {
            // Obtener el ID del usuario logueado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            int usuarioId = int.Parse(userId); // Cambiar si usas GUID

            // Inicializar variables de fecha
            DateTime? fechaInicio = null;
            DateTime? fechaFin = null;

            // Si viene el parámetro 'values', intentar deserializar
            if (!string.IsNullOrEmpty(values))
            {
                try
                {
                    var filtro = JsonSerializer.Deserialize<Dictionary<string, string>>(values);

                    if (filtro.ContainsKey("fechaInicio") && DateTime.TryParse(filtro["fechaInicio"], out DateTime fi))
                        fechaInicio = fi;

                    if (filtro.ContainsKey("fechaFin") && DateTime.TryParse(filtro["fechaFin"], out DateTime ff))
                        fechaFin = ff;
                }
                catch
                {
                    return BadRequest("Error al leer los filtros.");
                }
            }

            var movimientos = from m in _context.Movimientos
                              join f in _context.FondosMonetarios on m.FondoID equals f.FondoID
                              join u in _context.Usuarios on m.UsuarioID equals u.UsuarioID
                              where m.UsuarioID == usuarioId
                              select new MovimientoViewModel
                              {
                                  MovimientoID = m.MovimientoID,
                                  UsuarioID = m.UsuarioID,
                                  Fecha = m.Fecha,
                                  FondoID = m.FondoID,
                                  TipoMovimiento = m.TipoMovimiento,
                                  Monto = m.Monto,
                                  Observaciones = m.Observaciones,
                                  NombreComercio = m.NombreComercio,
                                  TipoDocumento = m.TipoDocumento,
                                  FondoNombre = f.Descripcion,
                                  UsuarioNombre = u.Nombre
                              };

            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                movimientos = movimientos.Where(m => m.Fecha >= fechaInicio && m.Fecha <= fechaFin);
            }

            return Json(DataSourceLoader.Load(movimientos, loadOptions));
        }

        [HttpGet]
        public async Task<IActionResult> ExportarExcel(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            int usuarioId = int.Parse(userId);

            var movimientos = from m in _context.Movimientos
                              join f in _context.FondosMonetarios on m.FondoID equals f.FondoID
                              join u in _context.Usuarios on m.UsuarioID equals u.UsuarioID
                              where m.UsuarioID == usuarioId
                              select new
                              {
                                  m.MovimientoID,
                                  m.Fecha,
                                  Fondo = f.Descripcion,
                                  m.TipoMovimiento,
                                  m.Monto,
                                  m.NombreComercio,
                                  m.TipoDocumento,
                                  m.Observaciones,
                                  Usuario = u.Nombre
                              };

            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                movimientos = movimientos.Where(m => m.Fecha >= fechaInicio && m.Fecha <= fechaFin);
            }

            var lista = await movimientos.ToListAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Movimientos");
            worksheet.Cell(1, 1).InsertTable(lista);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "MovimientosFiltrados.xlsx");
        }

    }
}
