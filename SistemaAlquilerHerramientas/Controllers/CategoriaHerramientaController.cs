using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAlquilerHerramientas.Models;

namespace SistemaAlquilerHerramientas.Controllers
{
    public class CategoriaHerramientaController : Controller
    {
        private readonly AlquilerHerramientasContext _context;

        public CategoriaHerramientaController(AlquilerHerramientasContext context)
        {
            _context = context;
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCategoria,NombreCategoria,Descripcion,Estado")] CategoriaHerramientum categoriaHerramientum)
        {
            if (ModelState.IsValid)
            {
                _context.Add(categoriaHerramientum);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
                    _context.Update(categoriaHerramientum);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriaHerramientumExists(categoriaHerramientum.IdCategoria))
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
            var categoriaHerramientum = await _context.CategoriaHerramienta.FindAsync(id);
            if (categoriaHerramientum != null)
            {
                _context.CategoriaHerramienta.Remove(categoriaHerramientum);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoriaHerramientumExists(int id)
        {
            return _context.CategoriaHerramienta.Any(e => e.IdCategoria == id);
        }
    }
}
