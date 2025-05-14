using DevExpressSqlserver.Infraestructure.Conexion;
using DevExpressSqlserver.Models.Entities;
using DevExpressSqlserver.Models.ModelViews;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IdentityModel.Claims;

namespace DevExpressSqlserver.Controllers
{
    public class MovimientoRegistroDeGastosController : Controller
    {
        private readonly SBERPDbContext _context;

        public MovimientoRegistroDeGastosController(SBERPDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public object Get(DataSourceLoadOptions loadOptions)
        {
            var usuarioIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (usuarioIdClaim == null)
            {
                return Unauthorized();
            }

            int usuarioId = int.Parse(usuarioIdClaim);
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
            return DataSourceLoader.Load(movimientos, loadOptions);
                    
        }

        [HttpPost]
        public IActionResult Post(string values)
        {
            try
            {
                var movimiento = new Movimiento();
                JsonConvert.PopulateObject(values, movimiento);

                // Obtener el ID del usuario logueado
                var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (usuarioIdClaim == null)
                    return Unauthorized();

                int usuarioId = int.Parse(usuarioIdClaim);
                movimiento.UsuarioID = usuarioId;

                // Validar el saldo solo si es un GASTO
                if (movimiento.TipoMovimiento.ToLower() == "gasto")
                {
                    // Buscar el fondo asociado al movimiento
                    var fondo = _context.FondosMonetarios
                        .FirstOrDefault(f => f.FondoID == movimiento.FondoID && f.UsuarioId == usuarioId);

                    if (fondo == null)
                    {
                        return BadRequest(new { error = "Fondo no encontrado para este usuario." });
                    }

                    if (movimiento.Monto > fondo.Saldo)
                    {
                        return BadRequest(new
                        {
                            error = $"El monto del gasto ({movimiento.Monto:C}) excede el saldo disponible en el fondo ({fondo.Saldo:C})."
                        });
                    }

                    // Restar el monto al saldo del fondo
                    fondo.Saldo -= movimiento.Monto;
                }
                else if (movimiento.TipoMovimiento.ToLower() == "deposito")
                {
                    // Si es un depósito, aumentar el saldo del fondo
                    var fondo = _context.FondosMonetarios
                        .FirstOrDefault(f => f.FondoID == movimiento.FondoID && f.UsuarioId == usuarioId);

                    if (fondo != null)
                    {
                        fondo.Saldo += movimiento.Monto;
                    }
                }

                // Guardar el movimiento
                _context.Movimientos.Add(movimiento);
                _context.SaveChanges();

                return Json(movimiento);
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new
                {
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = ex.Message
                });
            }
        }
        [HttpGet]
        public IActionResult GetFondos()
        {
            var fondos = _context.FondosMonetarios
                .Select(f => new
                {
                    f.FondoID,
                    f.Descripcion
                }).ToList();

            return Json(fondos);
        }

        [HttpPut]
        public IActionResult Put(int key, string values)
        {
            try
            {
                // Buscar el movimiento por ID
                var movimiento = _context.Movimientos.Find(key);
                if (movimiento == null)
                    return NotFound(); // Si no se encuentra el movimiento, devolver 404

                // Deserializar los valores del movimiento desde el JSON
                JsonConvert.PopulateObject(values, movimiento);

                // Obtener el ID del usuario logueado desde los claims
                var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (usuarioIdClaim == null)
                    return Unauthorized(); // Si no se encuentra el ID del usuario, devolver 401

                // Asignar el ID del usuario al movimiento (esto es útil si el usuario tiene permisos específicos)
                movimiento.UsuarioID = int.Parse(usuarioIdClaim);

                // Aquí se pueden agregar validaciones adicionales, como asegurarse de que el usuario tenga acceso
                // para modificar este movimiento si es necesario

                // Actualizar el movimiento en la base de datos
                _context.Entry(movimiento).CurrentValues.SetValues(movimiento);
                _context.SaveChanges();

                // Devolver el movimiento actualizado como respuesta
                return Json(movimiento);
            }
            catch (DbUpdateException ex)
            {
                // En caso de error al guardar en la base de datos
                return BadRequest(new
                {
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                // En caso de cualquier otro error
                return BadRequest(new
                {
                    error = ex.Message
                });
            }
        }

        [HttpDelete]
        public IActionResult Delete(int key)
        {
            var movimiento = _context.Movimientos.Find(key);
            if (movimiento == null)
                return NotFound();

            _context.Movimientos.Remove(movimiento);
            _context.SaveChanges();

            return Ok();
        }
    }
}

