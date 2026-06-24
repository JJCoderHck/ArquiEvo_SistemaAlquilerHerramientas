using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAlquilerHerramientas.Models;
using System.Text.RegularExpressions;

namespace SistemaAlquilerHerramientas.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuarioController : Controller
    {
        private readonly AlquilerHerramientasContext _context;
        private readonly ILogger<UsuarioController> _logger;

        public UsuarioController(AlquilerHerramientasContext context, ILogger<UsuarioController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Usuario
        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .ToListAsync();
            return View(usuarios);
        }

        // GET: Usuario/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(m => m.IdUsuario == id);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // GET: Usuario/Create
        public IActionResult Create()
        {
            ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Nombre");
            return View();
        }

        // POST: Usuario/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdUsuario,Nombre,Correo,Usuario1,Contrasena,Estado,IdRol")] Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Validar Nombre no vacío
                    if (string.IsNullOrWhiteSpace(usuario.Nombre))
                    {
                        ModelState.AddModelError("Nombre", "El nombre es requerido");
                        ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Nombre", usuario.IdRol);
                        return View(usuario);
                    }

                    // Validar Correo válido
                    if (string.IsNullOrWhiteSpace(usuario.Correo) || !Regex.IsMatch(usuario.Correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    {
                        ModelState.AddModelError("Correo", "El correo es inválido");
                        ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Nombre", usuario.IdRol);
                        return View(usuario);
                    }

                    // Validar Usuario1 no vacío
                    if (string.IsNullOrWhiteSpace(usuario.Usuario1))
                    {
                        ModelState.AddModelError("Usuario1", "El usuario es requerido");
                        ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Nombre", usuario.IdRol);
                        return View(usuario);
                    }

                    // Validar Contraseña (mínimo 6 caracteres)
                    if (string.IsNullOrWhiteSpace(usuario.Contrasena) || usuario.Contrasena.Length < 6)
                    {
                        ModelState.AddModelError("Contrasena", "La contraseña debe tener al menos 6 caracteres");
                        ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Nombre", usuario.IdRol);
                        return View(usuario);
                    }

                    // Verificar Usuario1 duplicado
                    var usuarioExistente = await _context.Usuarios
                        .FirstOrDefaultAsync(u => u.Usuario1 == usuario.Usuario1);

                    if (usuarioExistente != null)
                    {
                        ModelState.AddModelError("Usuario1", "Este usuario ya existe en el sistema");
                        ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Nombre", usuario.IdRol);
                        return View(usuario);
                    }

                    // Verificar Correo duplicado
                    var correoExistente = await _context.Usuarios
                        .FirstOrDefaultAsync(u => u.Correo == usuario.Correo);

                    if (correoExistente != null)
                    {
                        ModelState.AddModelError("Correo", "Este correo ya está registrado en el sistema");
                        ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Nombre", usuario.IdRol);
                        return View(usuario);
                    }

                    // Validar que el Rol existe
                    var rolExiste = await _context.Rols.AnyAsync(r => r.IdRol == usuario.IdRol);
                    if (!rolExiste)
                    {
                        ModelState.AddModelError("IdRol", "El rol seleccionado no existe");
                        ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Nombre", usuario.IdRol);
                        return View(usuario);
                    }

                    usuario.Estado = usuario.Estado ?? "Activo";
                    _context.Add(usuario);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Usuario creado: {usuario.Usuario1}");
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error de base de datos al crear usuario");
                    ModelState.AddModelError("", "Error al guardar en la base de datos");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inesperado al crear usuario");
                    ModelState.AddModelError("", "Error inesperado al crear el usuario");
                }
            }

            ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Nombre", usuario.IdRol);
            return View(usuario);
        }

        // GET: Usuario/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Nombre", usuario.IdRol);
            return View(usuario);
        }

        // POST: Usuario/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdUsuario,Nombre,Correo,Usuario1,Contrasena,Estado,IdRol")] Usuario usuario)
        {
            if (id != usuario.IdUsuario)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Validar Nombre no vacío
                    if (string.IsNullOrWhiteSpace(usuario.Nombre))
                    {
                        ModelState.AddModelError("Nombre", "El nombre es requerido");
                        ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Nombre", usuario.IdRol);
                        return View(usuario);
                    }

                    // Validar Correo válido
                    if (string.IsNullOrWhiteSpace(usuario.Correo) || !Regex.IsMatch(usuario.Correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    {
                        ModelState.AddModelError("Correo", "El correo es inválido");
                        ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Nombre", usuario.IdRol);
                        return View(usuario);
                    }

                    // Validar Usuario1 no vacío
                    if (string.IsNullOrWhiteSpace(usuario.Usuario1))
                    {
                        ModelState.AddModelError("Usuario1", "El usuario es requerido");
                        ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Nombre", usuario.IdRol);
                        return View(usuario);
                    }

                    // Validar Contraseña (mínimo 6 caracteres)
                    if (string.IsNullOrWhiteSpace(usuario.Contrasena) || usuario.Contrasena.Length < 6)
                    {
                        ModelState.AddModelError("Contrasena", "La contraseña debe tener al menos 6 caracteres");
                        ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Nombre", usuario.IdRol);
                        return View(usuario);
                    }

                    // Verificar Usuario1 duplicado (excluyendo el registro actual)
                    var usuarioExistente = await _context.Usuarios
                        .FirstOrDefaultAsync(u => u.Usuario1 == usuario.Usuario1 && u.IdUsuario != id);

                    if (usuarioExistente != null)
                    {
                        ModelState.AddModelError("Usuario1", "Este usuario ya existe en el sistema");
                        ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Nombre", usuario.IdRol);
                        return View(usuario);
                    }

                    // Verificar Correo duplicado (excluyendo el registro actual)
                    var correoExistente = await _context.Usuarios
                        .FirstOrDefaultAsync(u => u.Correo == usuario.Correo && u.IdUsuario != id);

                    if (correoExistente != null)
                    {
                        ModelState.AddModelError("Correo", "Este correo ya está registrado en el sistema");
                        ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Nombre", usuario.IdRol);
                        return View(usuario);
                    }

                    // Validar que el Rol existe
                    var rolExiste = await _context.Rols.AnyAsync(r => r.IdRol == usuario.IdRol);
                    if (!rolExiste)
                    {
                        ModelState.AddModelError("IdRol", "El rol seleccionado no existe");
                        ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Nombre", usuario.IdRol);
                        return View(usuario);
                    }

                    _context.Update(usuario);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Usuario actualizado: {usuario.Usuario1}");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.IdUsuario))
                    {
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError("Error de concurrencia al editar usuario");
                        ModelState.AddModelError("", "El usuario fue modificado por otro administrador");
                        ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Nombre", usuario.IdRol);
                        return View(usuario);
                    }
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error de base de datos al editar usuario");
                    ModelState.AddModelError("", "Error al guardar en la base de datos");
                    ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Nombre", usuario.IdRol);
                    return View(usuario);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inesperado al editar usuario");
                    ModelState.AddModelError("", "Error inesperado al editar el usuario");
                    ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Nombre", usuario.IdRol);
                    return View(usuario);
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["IdRol"] = new SelectList(_context.Rols, "IdRol", "Nombre", usuario.IdRol);
            return View(usuario);
        }

        // GET: Usuario/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(m => m.IdUsuario == id);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuario/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(id);
                if (usuario != null)
                {
                    _context.Usuarios.Remove(usuario);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Usuario eliminado: {usuario.Usuario1}");
                }

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error de base de datos al eliminar usuario");
                TempData["ErrorMessage"] = "Error al eliminar el usuario";
                return RedirectToAction(nameof(Delete), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al eliminar usuario");
                TempData["ErrorMessage"] = "Error inesperado al eliminar el usuario";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.IdUsuario == id);
        }
    }
}