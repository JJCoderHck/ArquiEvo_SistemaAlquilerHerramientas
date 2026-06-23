using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SistemaAlquilerHerramientas.Models;

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
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Buscar usuario incluyendo la navegación al Rol
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.Correo == model.Correo
                                  && u.Contrasena == model.Contrasena
                                  && u.Estado == "Activo");

            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
                return View(model);
            }

            // Obtener el nombre del rol usando la relación de navegación
            var rol = _context.Rols.FirstOrDefault(r => r.IdRol == usuario.IdRol);
            string nombreRol = rol?.NombreRol ?? "Cliente";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, nombreRol)
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

            if (nombreRol == "Administrador")
                return RedirectToAction("Index", "Home");

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}