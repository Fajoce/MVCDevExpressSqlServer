using DevExpressSqlserver.Infraestructure.Conexion;
using DevExpressSqlserver.Models.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DevExpressSqlserver.Models.ModelViews;

namespace DevExpressSqlserver.Controllers
{
   
    public class UsuariosController : Controller
    {
        private readonly SBERPDbContext _context;

        public UsuariosController(SBERPDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = _context.Usuarios
                    .FirstOrDefault(u => u.Login == model.Login && u.Password == model.Password);

                if (usuario != null)
                {
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioID.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim("Apellido", usuario.Apellido),
                new Claim("Login", usuario.Login ?? ""),
                new Claim("Correo", usuario.Correo ?? ""),
                new Claim("Direccion", usuario.Direccion ?? ""),
                new Claim("Telefono", usuario.Telefono ?? "")
            };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            }

            return View(model);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // Mostrar formulario de registro
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        // Procesar datos del formulario
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var exists = _context.Usuarios.Any(u => u.Login == model.Login);
                if (exists)
                {
                    ModelState.AddModelError("Login", "Ya existe un usuario con este login.");
                    return View(model);
                }

                var nuevoUsuario = new Usuario
                {
                    Identificacion = model.Identificacion,
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    Login = model.Login,
                    Password = model.Password,
                    FechaNacimiento = model.FechaNacimiento,
                    Direccion = model.Direccion,
                    Correo = model.Correo,
                    Telefono = model.Telefono
                };

                _context.Usuarios.Add(nuevoUsuario);
                _context.SaveChanges();

                return RedirectToAction("Login");
            }

            return View(model);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>

        [Authorize]
        public IActionResult EditarPerfil()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioID == userId);

            if (usuario == null)
            {
                return NotFound();
            }

            var model = new Usuario
            {
                UsuarioID = usuario.UsuarioID,
                Identificacion = usuario.Identificacion,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Login = usuario.Login,
                Correo = usuario.Correo,
                Password = usuario.Password,
                Direccion = usuario.Direccion,
                FechaNacimiento = usuario.FechaNacimiento,
                Telefono = usuario.Telefono
            };

            return View(model);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public IActionResult EditarPerfil(Usuario model)
        {
            if (ModelState.IsValid)
            {
                var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioID == model.UsuarioID);
                if (usuario != null)
                {
                    usuario.UsuarioID = model.UsuarioID;
                    usuario.Identificacion = model.Identificacion;
                    usuario.Nombre = model.Nombre;
                    usuario.Apellido = model.Apellido;
                    usuario.Login = model.Login;
                    usuario.Correo = model.Correo;
                    usuario.Password = model.Password;
                    usuario.Direccion = model.Direccion;
                    usuario.FechaNacimiento = model.FechaNacimiento;
                    usuario.Telefono = model.Telefono;

                    _context.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }

                return NotFound();
            }

            return View(model);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}

