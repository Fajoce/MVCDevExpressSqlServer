using DevExpressSqlserver.Infraestructure.Conexion;
using DevExpressSqlserver.Models.Entities;
using DevExpressSqlserver.Models.Guards;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace DevExpressSqlserver.Controllers
{
    public class TiposGastosController : Controller
    {
        private readonly SBERPDbContext _context;

        public TiposGastosController(SBERPDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize]
        
        public IActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="loadOptions"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public IActionResult GetData(DataSourceLoadOptions loadOptions)
        {
            // Obtener el ID del usuario logueado desde los claims
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();

            int userId = int.Parse(userIdString); // Asumiendo que el ID es entero
            var data = from tg in _context.TiposGasto where tg.UsuarioId == userId
                        select new TipoGasto
                        {
                            TipoGastoID = tg.TipoGastoID,
                            Descripcion = tg.Descripcion
                        };
            return Json(DataSourceLoader.Load(data, loadOptions));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>

        [HttpPost]
        public IActionResult AddNew(string values)
        {
            // Obtener el ID del usuario logueado desde los claims
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
                return Unauthorized();

            int userId = int.Parse(userIdString);
            try
            {              
                var tipoGasto = new TipoGasto();
                JsonConvert.PopulateObject(values, tipoGasto);

                tipoGasto.UsuarioId = userId;
                _context.TiposGasto.Add(tipoGasto);
                _context.SaveChanges();
          
                return Json(tipoGasto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns></returns>

        [HttpPut]
        public IActionResult Update(int key, string values)
        {
            try
            {
                var product = _context.TiposGasto.Find(key);
                if (product == null) return StatusCode(409, "Tipo de gasto no encontrado");

                JsonConvert.PopulateObject(values, product);

                // ProductGuard.Validate(product);
                //if (ModelState.IsValid)
                //{
                _context.SaveChanges();
                //}

                return Json(product);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete]
        public IActionResult Delete(int key)
        {
            var tipoGasto = _context.TiposGasto.Find(key);
            if (tipoGasto == null) return StatusCode(409, "Tipo de gasto no encontrado");

            _context.TiposGasto.Remove(tipoGasto);
            _context.SaveChanges();

            return Ok();
        }
    }
}

