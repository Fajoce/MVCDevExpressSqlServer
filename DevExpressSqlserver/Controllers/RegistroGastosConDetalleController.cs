using DevExpressSqlserver.Infraestructure.Conexion;
using DevExpressSqlserver.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Claims;
using System.Text.Json;

namespace DevExpressSqlserver.Controllers
{
    [Authorize]
    public class RegistroGastosConDetalleController : Controller
    {
        private readonly SBERPDbContext _context;

        public RegistroGastosConDetalleController(SBERPDbContext context)
        {
            _context = context;
        }

        // GET: Movimiento/Create

        public IActionResult Create()
        {
            int usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var tipoGastos = _context.TiposGasto
                .Where(t => t.UsuarioId == usuarioId)
                .Select(t => new { t.TipoGastoID, t.Descripcion} ) 
                .ToList();

            ViewData["FondosMonetarios"] = new SelectList(_context.FondosMonetarios, "FondoID", "Descripcion");
            ViewBag.TipoGastoListJson = JsonSerializer.Serialize(tipoGastos);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movimiento movimiento, string detallesJson)
        {
            var detalles = JsonSerializer.Deserialize<List<DetalleMovimiento>>(detallesJson);

            if (detalles == null || !detalles.Any())
            {
                ModelState.AddModelError("", "Debe agregar al menos un detalle.");
            }

            if (ModelState.IsValid)
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                movimiento.UsuarioID = usuarioId;
                movimiento.TipoMovimiento = "Gasto"; // o "Deposito"
                movimiento.Fecha = DateTime.Now;
                movimiento.Monto = detalles.Sum(d => d.Monto);
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.Movimientos.Add(movimiento);
                    await _context.SaveChangesAsync();

                    foreach (var detalle in detalles)
                    {
                        detalle.UsuarioId = usuarioId;
                        detalle.MovimientoID = movimiento.MovimientoID;
                        _context.DetalleMovimientos.Add(detalle);
                    }

                    await _context.SaveChangesAsync();

                    // Validar presupuesto por tipo de gasto
                    var tipoGastoIds = detalles.Select(d => d.TipoGastoID).Distinct();
                    var presupuestos = await _context.TiposGasto
                        .Where(t => tipoGastoIds.Contains(t.TipoGastoID) && t.UsuarioId == usuarioId)
                        .ToListAsync();

                    var alertas = new List<string>();

                    foreach (var tipo in presupuestos)
                    {
                        var montoGastado = detalles
                            .Where(d => d.TipoGastoID == tipo.TipoGastoID)
                            .Sum(d => d.Monto);

                        if (montoGastado > tipo.MontoPresupuestado)
                        {
                            alertas.Add($"Presupuesto sobregirado en '{tipo.Descripcion}': Presupuestado {tipo.MontoPresupuestado:C}, Gastado {montoGastado:C}");
                        }
                    }

                    if (alertas.Any())
                    {
                        ViewData["FondosMonetarios"] = new SelectList(_context.FondosMonetarios, "FondoID", "Descripcion");
                        var tipoGastos = _context.TiposGasto.Select(t => new { t.TipoGastoID, t.Descripcion }).ToList();
                        ViewBag.TipoGastoListJson = JsonSerializer.Serialize(tipoGastos);
                        ViewBag.Alertas = alertas;
                        return View(movimiento);
                    }

                    await transaction.CommitAsync();
                    return RedirectToAction("Create");
                }
                catch
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Error al guardar los datos.");
                }
            }
            foreach (var entry in ModelState)
            {
                foreach (var error in entry.Value.Errors)
                {
                    Console.WriteLine($"Error en el campo '{entry.Key}': {error.ErrorMessage}");
                }
            }
        
  
            ViewData["FondosMonetarios"] = new SelectList(_context.FondosMonetarios, "FondoID", "Descripcion", movimiento.FondoID);
            var tipoGastosReload = _context.TiposGasto.Select(t => new { t.TipoGastoID, t.Descripcion }).ToList();
            ViewBag.TipoGastoListJson = JsonSerializer.Serialize(tipoGastosReload);
            return View(movimiento);
        }
    }
}
