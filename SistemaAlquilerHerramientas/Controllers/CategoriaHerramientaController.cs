using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAlquilerHerramientas.Models;

namespace SistemaAlquilerHerramientas.Controllers
{
    public class CategoriaHerramientaController : Controller
    {
        private readonly AlquilerHerramientasContext _context;
        private readonly ILogger<CategoriaHerramientaController> _logger;

        public CategoriaHerramientaController(AlquilerHerramientasContext context, ILogger<CategoriaHerramientaController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: CategoriaHerramienta
        public async Task<IActionResult> Index()
        {
            return View(await _context.CategoriaHerramienta.ToListAsync());
        }

        // GET: CategoriaHerramienta/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoriaHerramientum = await _context.CategoriaHerramienta
                .FirstOrDefaultAsync(m => m.IdCategoria == id);

            if (categoriaHerramientum == null)
            {
                return NotFound();
            }

            return View(categoriaHerramientum);
        }

        // GET: CategoriaHerramienta/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CategoriaHerramienta/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCategoria,NombreCategoria,Descripcion,Estado")] CategoriaHerramientum categoriaHerramientum)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Validar NombreCategoria no vacío
                    if (string.IsNullOrWhiteSpace(categoriaHerramientum.NombreCategoria))
                    {
                        ModelState.AddModelError("NombreCategoria", "El nombre de la categoría es requerido");
                        return View(categoriaHerramientum);
                    }

                    // Validar que el nombre no sea muy largo (máximo 100 caracteres)
                    if (categoriaHerramientum.NombreCategoria.Length > 100)
                    {
                        ModelState.AddModelError("NombreCategoria", "El nombre de la categoría no puede exceder 100 caracteres");
                        return View(categoriaHerramientum);
                    }

                    // Verificar nombre duplicado (case-insensitive)
                    var nombreExistente = await _context.CategoriaHerramienta
                        .FirstOrDefaultAsync(c => c.NombreCategoria.ToLower() == categoriaHerramientum.NombreCategoria.ToLower());

                    if (nombreExistente != null)
                    {
                        ModelState.AddModelError("NombreCategoria", "Esta categoría ya existe en el sistema");
                        return View(categoriaHerramientum);
                    }

                    // Validar descripción si está presente (máximo 500 caracteres)
                    if (!string.IsNullOrWhiteSpace(categoriaHerramientum.Descripcion) && categoriaHerramientum.Descripcion.Length > 500)
                    {
                        ModelState.AddModelError("Descripcion", "La descripción no puede exceder 500 caracteres");
                        return View(categoriaHerramientum);
                    }

                    categoriaHerramientum.Estado = categoriaHerramientum.Estado ?? "Activo";
                    _context.Add(categoriaHerramientum);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Categoría creada: {categoriaHerramientum.NombreCategoria}");
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error de base de datos al crear categoría");
                    ModelState.AddModelError("", "Error al guardar en la base de datos");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inesperado al crear categoría");
                    ModelState.AddModelError("", "Error inesperado al crear la categoría");
                }
            }

            return View(categoriaHerramientum);
        }

        // GET: CategoriaHerramienta/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoriaHerramientum = await _context.CategoriaHerramienta.FindAsync(id);
            if (categoriaHerramientum == null)
            {
                return NotFound();
            }

            return View(categoriaHerramientum);
        }

        // POST: CategoriaHerramienta/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCategoria,NombreCategoria,Descripcion,Estado")] CategoriaHerramientum categoriaHerramientum)
        {
            if (id != categoriaHerramientum.IdCategoria)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Validar NombreCategoria no vacío
                    if (string.IsNullOrWhiteSpace(categoriaHerramientum.NombreCategoria))
                    {
                        ModelState.AddModelError("NombreCategoria", "El nombre de la categoría es requerido");
                        return View(categoriaHerramientum);
                    }

                    // Validar que el nombre no sea muy largo (máximo 100 caracteres)
                    if (categoriaHerramientum.NombreCategoria.Length > 100)
                    {
                        ModelState.AddModelError("NombreCategoria", "El nombre de la categoría no puede exceder 100 caracteres");
                        return View(categoriaHerramientum);
                    }

                    // Verificar nombre duplicado (excluyendo el registro actual, case-insensitive)
                    var nombreExistente = await _context.CategoriaHerramienta
                        .FirstOrDefaultAsync(c => c.NombreCategoria.ToLower() == categoriaHerramientum.NombreCategoria.ToLower()
                                                && c.IdCategoria != id);

                    if (nombreExistente != null)
                    {
                        ModelState.AddModelError("NombreCategoria", "Esta categoría ya existe en el sistema");
                        return View(categoriaHerramientum);
                    }

                    // Validar descripción si está presente (máximo 500 caracteres)
                    if (!string.IsNullOrWhiteSpace(categoriaHerramientum.Descripcion) && categoriaHerramientum.Descripcion.Length > 500)
                    {
                        ModelState.AddModelError("Descripcion", "La descripción no puede exceder 500 caracteres");
                        return View(categoriaHerramientum);
                    }

                    _context.Update(categoriaHerramientum);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Categoría actualizada: {categoriaHerramientum.NombreCategoria}");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriaHerramientumExists(categoriaHerramientum.IdCategoria))
                    {
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError("Error de concurrencia al editar categoría");
                        ModelState.AddModelError("", "La categoría fue modificada por otro usuario");
                        return View(categoriaHerramientum);
                    }
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error de base de datos al editar categoría");
                    ModelState.AddModelError("", "Error al guardar en la base de datos");
                    return View(categoriaHerramientum);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inesperado al editar categoría");
                    ModelState.AddModelError("", "Error inesperado al editar la categoría");
                    return View(categoriaHerramientum);
                }

                return RedirectToAction(nameof(Index));
            }

            return View(categoriaHerramientum);
        }

        // GET: CategoriaHerramienta/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoriaHerramientum = await _context.CategoriaHerramienta
                .FirstOrDefaultAsync(m => m.IdCategoria == id);

            if (categoriaHerramientum == null)
            {
                return NotFound();
            }

            return View(categoriaHerramientum);
        }

        // POST: CategoriaHerramienta/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var categoriaHerramientum = await _context.CategoriaHerramienta.FindAsync(id);
                if (categoriaHerramientum != null)
                {
                    // Verificar si tiene herramientas asociadas
                    var tieneHerramientas = await _context.Herramienta
                        .AnyAsync(h => h.IdCategoria == id);

                    if (tieneHerramientas)
                    {
                        _logger.LogWarning($"Intento de eliminar categoría con herramientas: {categoriaHerramientum.NombreCategoria}");
                        TempData["ErrorMessage"] = "No se puede eliminar esta categoría porque tiene herramientas asociadas";
                        return RedirectToAction(nameof(Index));
                    }

                    _context.CategoriaHerramienta.Remove(categoriaHerramientum);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Categoría eliminada: {categoriaHerramientum.NombreCategoria}");
                }

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error de base de datos al eliminar categoría");
                TempData["ErrorMessage"] = "Error al eliminar la categoría";
                return RedirectToAction(nameof(Delete), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al eliminar categoría");
                TempData["ErrorMessage"] = "Error inesperado al eliminar la categoría";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        private bool CategoriaHerramientumExists(int id)
        {
            return _context.CategoriaHerramienta.Any(e => e.IdCategoria == id);
        }
    }
}