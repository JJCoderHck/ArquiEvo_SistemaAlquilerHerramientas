using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAlquilerHerramientas.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaAlquilerHerramientas.Controllers
{
    [Authorize(Roles = "Administrador,Cliente")]
    public class HerramientasController : Controller
    {
        private readonly AlquilerHerramientasContext _context;
        private readonly IWebHostEnvironment _environment;

        public HerramientasController(AlquilerHerramientasContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Herramientas
        public async Task<IActionResult> Index()
        {
            var alquilerHerramientasContext = _context.Herramienta.Include(h => h.IdCategoriaNavigation).Include(h => h.IdProveedorNavigation);
            return View(await alquilerHerramientasContext.ToListAsync());
        }

        // GET: Herramientas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var herramientum = await _context.Herramienta
                .Include(h => h.IdCategoriaNavigation)
                .Include(h => h.IdProveedorNavigation)
                .FirstOrDefaultAsync(m => m.IdHerramienta == id);
            if (herramientum == null)
            {
                return NotFound();
            }

            return View(herramientum);
        }

        public IActionResult Imagen(int id)
        {
            var imagePath = FindHerramientaImagePath(id);
            if (imagePath == null)
            {
                imagePath = Path.Combine(_environment.WebRootPath, "images", "no-image.png");
            }

            return PhysicalFile(imagePath, GetContentType(Path.GetExtension(imagePath)));
        }

        // GET: Herramientas/Create
        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            PopulateHerramientaSelects();
            return View();
        }

        // POST: Herramientas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create([Bind("IdHerramienta,Nombre,Descripcion,PrecioPorDia,EstadoHerramienta,IdCategoria,IdProveedor")] Herramientum herramientum, string? nombreImagen, IFormFile? imagenProducto)
        {
            ValidarImagen(imagenProducto);

            if (ModelState.IsValid)
            {
                _context.Add(herramientum);
                await _context.SaveChangesAsync();
                await GuardarImagenHerramientaAsync(herramientum.IdHerramienta, herramientum.Nombre, nombreImagen, imagenProducto);
                return RedirectToAction(nameof(Index));
            }
            PopulateHerramientaSelects(herramientum);
            return View(herramientum);
        }

        // GET: Herramientas/Edit/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var herramientum = await _context.Herramienta.FindAsync(id);
            if (herramientum == null)
            {
                return NotFound();
            }
            PopulateHerramientaSelects(herramientum);
            return View(herramientum);
        }

        // POST: Herramientas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int id, [Bind("IdHerramienta,Nombre,Descripcion,PrecioPorDia,EstadoHerramienta,IdCategoria,IdProveedor")] Herramientum herramientum, string? nombreImagen, IFormFile? imagenProducto)
        {
            if (id != herramientum.IdHerramienta)
            {
                return NotFound();
            }

            ValidarImagen(imagenProducto);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(herramientum);
                    await _context.SaveChangesAsync();
                    await GuardarImagenHerramientaAsync(herramientum.IdHerramienta, herramientum.Nombre, nombreImagen, imagenProducto);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HerramientumExists(herramientum.IdHerramienta))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateHerramientaSelects(herramientum);
            return View(herramientum);
        }

        // GET: Herramientas/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var herramientum = await _context.Herramienta
                .Include(h => h.IdCategoriaNavigation)
                .Include(h => h.IdProveedorNavigation)
                .FirstOrDefaultAsync(m => m.IdHerramienta == id);
            if (herramientum == null)
            {
                return NotFound();
            }

            return View(herramientum);
        }

        // POST: Herramientas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var herramientum = await _context.Herramienta.FindAsync(id);
            if (herramientum != null)
            {
                _context.Herramienta.Remove(herramientum);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HerramientumExists(int id)
        {
            return _context.Herramienta.Any(e => e.IdHerramienta == id);
        }

        private void PopulateHerramientaSelects(Herramientum? herramientum = null)
        {
            var categorias = _context.CategoriaHerramienta
                .OrderBy(c => c.NombreCategoria)
                .Select(c => new
                {
                    c.IdCategoria,
                    Descripcion = c.IdCategoria + " - " + c.NombreCategoria
                })
                .ToList();

            var proveedores = _context.Proveedors
                .OrderBy(p => p.RazonSocial)
                .Select(p => new
                {
                    p.IdProveedor,
                    Descripcion = p.IdProveedor + " - " + p.RazonSocial
                })
                .ToList();

            ViewData["IdCategoria"] = new SelectList(categorias, "IdCategoria", "Descripcion", herramientum?.IdCategoria);
            ViewData["IdProveedor"] = new SelectList(proveedores, "IdProveedor", "Descripcion", herramientum?.IdProveedor);
        }

        private async Task GuardarImagenHerramientaAsync(int idHerramienta, string nombreHerramienta, string? nombreImagen, IFormFile? imagenProducto)
        {
            if (imagenProducto == null || imagenProducto.Length == 0)
                return;

            var imageFolder = GetHerramientaImageFolder();
            Directory.CreateDirectory(imageFolder);

            foreach (var existingFile in Directory.GetFiles(imageFolder, $"{idHerramienta}.*")
                         .Concat(Directory.GetFiles(imageFolder, $"{idHerramienta}-*.*")))
            {
                System.IO.File.Delete(existingFile);
            }

            var extension = Path.GetExtension(imagenProducto.FileName).ToLowerInvariant();
            var safeName = Slugify(string.IsNullOrWhiteSpace(nombreImagen) ? nombreHerramienta : nombreImagen);
            var fileName = $"{idHerramienta}-{safeName}{extension}";
            var path = Path.Combine(imageFolder, fileName);

            await using var stream = new FileStream(path, FileMode.Create);
            await imagenProducto.CopyToAsync(stream);
        }

        private void ValidarImagen(IFormFile? imagenProducto)
        {
            if (imagenProducto == null || imagenProducto.Length == 0)
                return;

            var extension = Path.GetExtension(imagenProducto.FileName).ToLowerInvariant();
            var allowedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".webp" };

            if (!allowedExtensions.Contains(extension))
                ModelState.AddModelError("imagenProducto", "La imagen debe ser JPG, PNG o WEBP.");

            if (imagenProducto.Length > 3 * 1024 * 1024)
                ModelState.AddModelError("imagenProducto", "La imagen no debe superar los 3 MB.");
        }

        private string? FindHerramientaImagePath(int idHerramienta)
        {
            var searchFolders = new[]
            {
                GetHerramientaImageFolder(),
                Path.Combine(_environment.WebRootPath, "images", "herramientas")
            };

            foreach (var folder in searchFolders.Where(Directory.Exists))
            {
                var image = Directory.GetFiles(folder, $"{idHerramienta}.*")
                    .Concat(Directory.GetFiles(folder, $"{idHerramienta}-*.*"))
                    .FirstOrDefault(IsSupportedImage);

                if (image != null)
                    return image;
            }

            return null;
        }

        private string GetHerramientaImageFolder()
        {
            return Path.Combine(_environment.WebRootPath, "img", "herramientas");
        }

        private static bool IsSupportedImage(string path)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();
            return extension is ".jpg" or ".jpeg" or ".png" or ".webp";
        }

        private static string GetContentType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".webp" => "image/webp",
                _ => "image/png"
            };
        }

        private static string Slugify(string value)
        {
            var normalized = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();

            foreach (var character in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(character);
                if (category == UnicodeCategory.NonSpacingMark)
                    continue;

                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(char.ToLowerInvariant(character));
                }
                else if (builder.Length > 0 && builder[^1] != '-')
                {
                    builder.Append('-');
                }
            }

            var result = builder.ToString().Trim('-');
            return string.IsNullOrWhiteSpace(result) ? "herramienta" : result;
        }
    }
}
