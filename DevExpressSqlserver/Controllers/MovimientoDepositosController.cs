using DevExpressSqlserver.Infraestructure.Conexion;
using DevExpressSqlserver.Models.Entities;
using DevExpressSqlserver.Models.ModelViews;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IdentityModel.Claims;
using System.Security.Claims;

namespace DevExpressSqlserver.Controllers
{
    public class MovimientoDepositosController : Controller
    {
        private readonly SBERPDbContext _context;

        public MovimientoDepositosController(SBERPDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public object Get(DataSourceLoadOptions loadOptions)
        {
            var userIdString = User.FindFirstValue(System.IdentityModel.Claims.ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();

            int userId = int.Parse(userIdString); // Asumiendo que el ID es entero
            var movimientos = from dm in _context.DetalleMovimientos
                              join tg in _context.TiposGasto on dm.TipoGastoID equals tg.TipoGastoID
                           where dm.UsuarioId == userId
                              select new MovimientoDepositoViewModel
                              {
                                  DetalleID = dm.DetalleID,
                                  MovimientoID = dm.MovimientoID,
                                  TipoGastoID = dm.TipoGastoID,
                                  NombreGasto = tg.Descripcion,
                                  Monto = dm.Monto

                              };
            Console.WriteLine(movimientos);
            return DataSourceLoader.Load(movimientos, loadOptions);

        }

        [HttpPost]
        public IActionResult Post(string values)
        {
            try
            {
                // Crear una nueva instancia de Movimiento
                var movimiento = new DetalleMovimiento();

                // Deserializar los datos JSON en el objeto Movimiento
                JsonConvert.PopulateObject(values, movimiento);

                // Obtener el ID del usuario logueado desde los claims
                var usuarioIdClaim = User.FindFirst(System.IdentityModel.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (usuarioIdClaim == null)
                    return Unauthorized(); // Si no se encuentra el ID del usuario, devolver 401

                // Asignar el ID del usuario al movimiento
                movimiento.UsuarioId = int.Parse(usuarioIdClaim);

                // Añadir el nuevo movimiento a la base de datos
                _context.DetalleMovimientos.Add(movimiento);
                _context.SaveChanges();

                // Devolver el objeto Movimiento como respuesta
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
                var movimiento = _context.DetalleMovimientos.Find(key);
                if (movimiento == null)
                    return NotFound(); // Si no se encuentra el movimiento, devolver 404

                // Deserializar los valores del movimiento desde el JSON
                JsonConvert.PopulateObject(values, movimiento);

                // Obtener el ID del usuario logueado desde los claims
                //var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                //if (usuarioIdClaim == null)
                   // return Unauthorized(); // Si no se encuentra el ID del usuario, devolver 401

                // Asignar el ID del usuario al movimiento (esto es útil si el usuario tiene permisos específicos)
               // movimiento.UsuarioID = int.Parse(usuarioIdClaim);

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
            var movimiento = _context.DetalleMovimientos.Find(key);
            if (movimiento == null)
                return NotFound();

            _context.DetalleMovimientos.Remove(movimiento);
            _context.SaveChanges();

            return Ok();
        }
        [HttpGet]
        public IActionResult GetTiposGasto()
        {
            var tipos = _context.TiposGasto.Select(t => new {
                t.TipoGastoID,
                t.Descripcion
            });
            return Json(tipos);
        }
    }
}

