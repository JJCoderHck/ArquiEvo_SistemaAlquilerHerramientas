using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAlquilerHerramientas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaAlquilerHerramientas.Controllers
{
    [Authorize(Roles = "Administrador,Cliente")]
    public class HerramientasController : Controller
    {
        private readonly AlquilerHerramientasContext _context;

        public HerramientasController(AlquilerHerramientasContext context)
        {
            _context = context;
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
        public async Task<IActionResult> Create([Bind("IdHerramienta,Nombre,Descripcion,PrecioPorDia,EstadoHerramienta,IdCategoria,IdProveedor")] Herramientum herramientum)
        {
            if (ModelState.IsValid)
            {
                _context.Add(herramientum);
                await _context.SaveChangesAsync();
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
        public async Task<IActionResult> Edit(int id, [Bind("IdHerramienta,Nombre,Descripcion,PrecioPorDia,EstadoHerramienta,IdCategoria,IdProveedor")] Herramientum herramientum)
        {
            if (id != herramientum.IdHerramienta)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(herramientum);
                    await _context.SaveChangesAsync();
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
    }
}
