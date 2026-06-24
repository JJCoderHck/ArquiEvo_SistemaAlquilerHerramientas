using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAlquilerHerramientas.Models;
using System.Text.RegularExpressions;

namespace SistemaAlquilerHerramientas.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ProveedorController : Controller
    {
        private readonly AlquilerHerramientasContext _context;
        private readonly ILogger<ProveedorController> _logger;

        public ProveedorController(AlquilerHerramientasContext context, ILogger<ProveedorController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Proveedor
        public async Task<IActionResult> Index()
        {
            return View(await _context.Proveedors.ToListAsync());
        }

        // GET: Proveedor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proveedor = await _context.Proveedors
                .FirstOrDefaultAsync(m => m.IdProveedor == id);
            if (proveedor == null)
            {
                return NotFound();
            }

            return View(proveedor);
        }

        // GET: Proveedor/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Proveedor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdProveedor,RazonSocial,Ruc,Telefono,Correo,Direccion,Estado")] Proveedor proveedor)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Validar que Razón Social no sea vacía
                    if (string.IsNullOrWhiteSpace(proveedor.RazonSocial))
                    {
                        ModelState.AddModelError("RazonSocial", "La razón social es requerida");
                        return View(proveedor);
                    }

                    // Validar RUC (11 dígitos)
                    if (string.IsNullOrWhiteSpace(proveedor.Ruc) || !Regex.IsMatch(proveedor.Ruc, @"^\d{11}$"))
                    {
                        ModelState.AddModelError("Ruc", "El RUC debe tener 11 dígitos");
                        return View(proveedor);
                    }

                    // Verificar RUC duplicado
                    var rucExistente = await _context.Proveedors
                        .FirstOrDefaultAsync(p => p.Ruc == proveedor.Ruc);

                    if (rucExistente != null)
                    {
                        ModelState.AddModelError("Ruc", "Este RUC ya está registrado en el sistema");
                        return View(proveedor);
                    }

                    // Validar teléfono si está presente (9 dígitos)
                    if (!string.IsNullOrWhiteSpace(proveedor.Telefono) && !Regex.IsMatch(proveedor.Telefono, @"^\d{9}$"))
                    {
                        ModelState.AddModelError("Telefono", "El teléfono debe tener 9 dígitos");
                        return View(proveedor);
                    }

                    // Validar correo si está presente
                    if (!string.IsNullOrWhiteSpace(proveedor.Correo) && !Regex.IsMatch(proveedor.Correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    {
                        ModelState.AddModelError("Correo", "El correo es inválido");
                        return View(proveedor);
                    }

                    // Asignar estado por defecto si no viene
                    proveedor.Estado = proveedor.Estado ?? "Activo";

                    _context.Add(proveedor);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Proveedor creado: {proveedor.RazonSocial} (RUC: {proveedor.Ruc})");
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error de base de datos al crear proveedor");
                    ModelState.AddModelError("", "Error al guardar en la base de datos");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inesperado al crear proveedor");
                    ModelState.AddModelError("", "Error inesperado al crear el proveedor");
                }
            }

            return View(proveedor);
        }

        // GET: Proveedor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proveedor = await _context.Proveedors.FindAsync(id);
            if (proveedor == null)
            {
                return NotFound();
            }
            return View(proveedor);
        }

        // POST: Proveedor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdProveedor,RazonSocial,Ruc,Telefono,Correo,Direccion,Estado")] Proveedor proveedor)
        {
            if (id != proveedor.IdProveedor)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Validar que Razón Social no sea vacía
                    if (string.IsNullOrWhiteSpace(proveedor.RazonSocial))
                    {
                        ModelState.AddModelError("RazonSocial", "La razón social es requerida");
                        return View(proveedor);
                    }

                    // Validar RUC (11 dígitos)
                    if (string.IsNullOrWhiteSpace(proveedor.Ruc) || !Regex.IsMatch(proveedor.Ruc, @"^\d{11}$"))
                    {
                        ModelState.AddModelError("Ruc", "El RUC debe tener 11 dígitos");
                        return View(proveedor);
                    }

                    // Verificar RUC duplicado (excluyendo el registro actual)
                    var rucExistente = await _context.Proveedors
                        .FirstOrDefaultAsync(p => p.Ruc == proveedor.Ruc && p.IdProveedor != id);

                    if (rucExistente != null)
                    {
                        ModelState.AddModelError("Ruc", "Este RUC ya está registrado en el sistema");
                        return View(proveedor);
                    }

                    // Validar teléfono si está presente (9 dígitos)
                    if (!string.IsNullOrWhiteSpace(proveedor.Telefono) && !Regex.IsMatch(proveedor.Telefono, @"^\d{9}$"))
                    {
                        ModelState.AddModelError("Telefono", "El teléfono debe tener 9 dígitos");
                        return View(proveedor);
                    }

                    // Validar correo si está presente
                    if (!string.IsNullOrWhiteSpace(proveedor.Correo) && !Regex.IsMatch(proveedor.Correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    {
                        ModelState.AddModelError("Correo", "El correo es inválido");
                        return View(proveedor);
                    }

                    _context.Update(proveedor);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Proveedor actualizado: {proveedor.RazonSocial} (RUC: {proveedor.Ruc})");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProveedorExists(proveedor.IdProveedor))
                    {
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError("Error de concurrencia al editar proveedor");
                        ModelState.AddModelError("", "El proveedor fue modificado por otro usuario");
                    }
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error de base de datos al editar proveedor");
                    ModelState.AddModelError("", "Error al guardar en la base de datos");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inesperado al editar proveedor");
                    ModelState.AddModelError("", "Error inesperado al editar el proveedor");
                }

                if (!ModelState.IsValid)
                {
                    return View(proveedor);
                }

                return RedirectToAction(nameof(Index));
            }

            return View(proveedor);
        }

        // GET: Proveedor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proveedor = await _context.Proveedors
                .FirstOrDefaultAsync(m => m.IdProveedor == id);
            if (proveedor == null)
            {
                return NotFound();
            }

            return View(proveedor);
        }

        // POST: Proveedor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var proveedor = await _context.Proveedors.FindAsync(id);
                if (proveedor != null)
                {
                    // Verificar si tiene herramientas asociadas
                    var tieneHerramientas = await _context.Herramienta
                        .AnyAsync(h => h.IdProveedor == id);

                    if (tieneHerramientas)
                    {
                        _logger.LogWarning($"Intento de eliminar proveedor con herramientas: {proveedor.RazonSocial}");
                        TempData["ErrorMessage"] = "No se puede eliminar este proveedor porque tiene herramientas asociadas";
                        return RedirectToAction(nameof(Index));
                    }

                    _context.Proveedors.Remove(proveedor);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Proveedor eliminado: {proveedor.RazonSocial}");
                }

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error de base de datos al eliminar proveedor");
                TempData["ErrorMessage"] = "Error al eliminar el proveedor";
                return RedirectToAction(nameof(Delete), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al eliminar proveedor");
                TempData["ErrorMessage"] = "Error inesperado al eliminar el proveedor";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        private bool ProveedorExists(int id)
        {
            return _context.Proveedors.Any(e => e.IdProveedor == id);
        }
    }
}