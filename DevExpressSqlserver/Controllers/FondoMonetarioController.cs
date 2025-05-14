using DevExpressSqlserver.Infraestructure.Conexion;
using DevExpressSqlserver.Models.Entities;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace DevExpressSqlserver.Controllers
{
    public class FondoMonetarioController : Controller
    {
        private readonly SBERPDbContext _context;

        public FondoMonetarioController(SBERPDbContext context)
        {
            _context = context;
        }
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [Authorize]
        public IActionResult GetData(DataSourceLoadOptions loadOptions)
        {
            // Obtener el ID del usuario logueado desde los claims
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();

            int userId = int.Parse(userIdString); // Asumiendo que el ID es entero
            var data = (from fm in _context.FondosMonetarios where fm.UsuarioId == userId
                        select new FondosMonetarios
                        {
                            FondoID = fm.FondoID,
                            Descripcion = fm.Descripcion,
                            Saldo = fm.Saldo
                        });
            return Json(DataSourceLoader.Load(data, loadOptions));
        }

        [HttpPost]
        public IActionResult AddNew(string values)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();

            int userId = int.Parse(userIdString); //
            try
            {
                var fm = new FondosMonetarios();
                JsonConvert.PopulateObject(values, fm);

                // ProductGuard.Validate(product);
                //if (ProductGuard.IsValid)
                //{
                fm.UsuarioId = userId;
                _context.FondosMonetarios.Add(fm);
                _context.SaveChanges();
                //}
                return Json(fm);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut]
        public IActionResult Update(int key, string values)
        {
            try
            {
                var fm = _context.FondosMonetarios.Find(key);
                if (fm == null) return StatusCode(409, "Fondo monetario no encontrado");

                JsonConvert.PopulateObject(values, fm);

                // ProductGuard.Validate(product);
                //if (ModelState.IsValid)
                //{
                _context.SaveChanges();
                //}

                return Json(fm);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete]
        public IActionResult Delete(int key)
        {
            var fm = _context.FondosMonetarios.Find(key);
            if (fm == null) return StatusCode(409, "Tipo de gasto no encontrado");

            _context.FondosMonetarios.Remove(fm);
            _context.SaveChanges();

            return Ok();
        }
    }
}

