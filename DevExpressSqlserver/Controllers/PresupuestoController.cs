using DevExpressSqlserver.Infraestructure.Conexion;
using DevExpressSqlserver.Models.Entities;
using DevExpressSqlserver.Models.ModelViews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IdentityModel.Claims;
using System.Text.RegularExpressions;

namespace DevExpressSqlserver.Controllers
{
    [Authorize]
    public class PresupuestoController : Controller
    {
        private readonly SBERPDbContext _context;

        public PresupuestoController(SBERPDbContext context)
        {
            _context = context;
        }

        // Vista principal
        public IActionResult Index()
        {
            return View();
        }

        // DevExtreme: Obtener datos del usuario actual
        public IActionResult GetData()
        {
            var usuarioIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (usuarioIdClaim == null)
            {
                return Unauthorized();
            }

            int usuarioId = int.Parse(usuarioIdClaim);

            var data = (from p in  _context.Presupuestos  join tg in _context.TiposGasto
                       on p.TipoGastoID equals tg.TipoGastoID where p.UsuarioID == usuarioId
                select new PresupuestoModelView
                {
                   PresupuestoID = p.PresupuestoID,
                   UsuarioID = p.UsuarioID,
                   TipoGastoID = p.TipoGastoID,
                   Monto = p.Monto,
                   Mes = p.Mes,
                   NombreGasto = tg.Descripcion
                })
                .ToList();

            return Json(data);
        }
        [HttpPost]
        public IActionResult Insert(string values)
        {
            try
            {
                var presupuesto = new Presupuesto();
                JsonConvert.PopulateObject(values, presupuesto);

                var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (usuarioIdClaim == null)
                    return Unauthorized();

                presupuesto.UsuarioID = int.Parse(usuarioIdClaim);
               
                _context.Presupuestos.Add(presupuesto);
                _context.SaveChanges();

                return Json(presupuesto);
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

        [HttpPut]
        public IActionResult Update(int key, [FromBody] Presupuesto values)
        {
            var presupuesto = _context.Presupuestos.Find(key);
            if (presupuesto == null) return NotFound();

            presupuesto.Mes = values.Mes;
            presupuesto.Monto = values.Monto;
            presupuesto.TipoGastoID = values.TipoGastoID;

            _context.SaveChanges();
            return Json(presupuesto);
        }

        [HttpDelete]
        public IActionResult Delete(int key)
        {
            var presupuesto = _context.Presupuestos.Find(key);
            if (presupuesto == null) return NotFound();

            _context.Presupuestos.Remove(presupuesto);
            _context.SaveChanges();
            return Ok();
        }

        // Usado para cargar Tipos de Gasto al editar
        public IActionResult GetTiposGasto()
        {
            var tipos = _context.TiposGasto.Select(t => new {
                t.TipoGastoID,
                t.Descripcion
            });
            return Json(tipos);
        }
        [HttpGet]
        public IActionResult ShowTiposGasto()
        {
            var tipos = _context.TiposGasto
          .Select(t => new {
              t.TipoGastoID,        // ValueExpr debe ser exactamente este nombre
              t.Descripcion         // DisplayExpr debe ser exactamente este nombre
          })
          .ToList();

            return Json(tipos);
        }
    }
}

