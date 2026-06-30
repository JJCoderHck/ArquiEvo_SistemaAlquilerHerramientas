using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAlquilerHerramientas.Models;
using System.Security.Claims;

namespace SistemaAlquilerHerramientas.Controllers
{
    public class AccountController : Controller
    {
        private readonly AlquilerHerramientasContext _context;

        public AccountController(AlquilerHerramientasContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // BUG CORREGIDO: antes se hacía SignOutAsync aquí, lo que cerraba
            // la sesión activa cada vez que alguien visitaba /Account/Login
            // (por ejemplo, al navegar hacia atrás tras iniciar sesión como admin).
            // Ahora: si ya está autenticado, redirigir directo al dashboard.
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Dashboard", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var identificador = model.Correo.Trim();
            var contrasena = model.Contrasena.Trim();

            // BUG CORREGIDO: el filtro anterior exigía u.Estado.Trim().ToUpper() == "ACTIVO",
            // pero si la BD tiene Estado = NULL o vacío (común en el usuario admin recién creado),
            // la consulta no devuelve nada y el login falla con un error opaco.
            // Solución: primero buscamos por credenciales, luego validamos el estado por separado
            // para poder dar un mensaje de error más claro.
            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u =>
                    (u.Correo.Trim() == identificador || u.Usuario1.Trim() == identificador)
                    && u.Contrasena == contrasena);

            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "Correo/usuario o contraseña incorrectos.");
                return View(model);
            }

            // Validar estado separado para dar mensaje preciso
            if (usuario.Estado != null &&
                usuario.Estado.Trim().ToUpper() == "INACTIVO")
            {
                ModelState.AddModelError(string.Empty, "Tu cuenta está inactiva. Contacta al administrador.");
                return View(model);
            }

            // Construir claims de sesión
            string nombreRol = usuario.IdRolNavigation?.NombreRol ?? "Cliente";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name,           usuario.Nombre),
                new Claim(ClaimTypes.Email,          usuario.Correo),
                new Claim(ClaimTypes.Role,           nombreRol)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Dashboard", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        public IActionResult AccessDenied()
        {
            return View("AccesoDenegado");
        }
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Dashboard", "Home");

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Dashboard", "Home");

            model.Correo = (model.Correo ?? string.Empty).Trim();
            model.Usuario = (model.Usuario ?? string.Empty).Trim();
            model.Nombres = (model.Nombres ?? string.Empty).Trim();
            model.Apellidos = (model.Apellidos ?? string.Empty).Trim();
            model.Dni = (model.Dni ?? string.Empty).Trim();

            if (await _context.Usuarios.AnyAsync(u => u.Correo == model.Correo))
                ModelState.AddModelError(nameof(model.Correo), "Ya existe un usuario registrado con este correo.");

            if (await _context.Usuarios.AnyAsync(u => u.Usuario1 == model.Usuario))
                ModelState.AddModelError(nameof(model.Usuario), "El nombre de usuario ya esta en uso.");

            if (await _context.Clientes.AnyAsync(c => c.Dni == model.Dni))
                ModelState.AddModelError(nameof(model.Dni), "Ya existe un cliente registrado con este DNI.");

            if (!ModelState.IsValid)
                return View(model);

            var rolCliente = await _context.Rols
                .FirstOrDefaultAsync(r => r.NombreRol == "Cliente");

            if (rolCliente == null)
            {
                rolCliente = new Rol
                {
                    NombreRol = "Cliente",
                    Descripcion = "Usuario cliente del sistema"
                };
                _context.Rols.Add(rolCliente);
                await _context.SaveChangesAsync();
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var cliente = new Cliente
                {
                    Nombres = model.Nombres,
                    Apellidos = model.Apellidos,
                    Dni = model.Dni,
                    Telefono = model.Telefono?.Trim(),
                    Correo = model.Correo,
                    Direccion = model.Direccion?.Trim(),
                    Estado = "Activo"
                };

                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();

                var usuario = new Usuario
                {
                    Nombre = $"{model.Nombres} {model.Apellidos}".Trim(),
                    Correo = model.Correo,
                    Usuario1 = model.Usuario,
                    Contrasena = model.Contrasena,
                    Estado = "Activo",
                    IdRol = rolCliente.IdRol
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Registro completado correctamente. Ahora puedes iniciar sesion.";
                return RedirectToAction(nameof(Login));
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, $"No se pudo completar el registro: {ex.GetBaseException().Message}");
                return View(model);
            }
        }

    }
}
