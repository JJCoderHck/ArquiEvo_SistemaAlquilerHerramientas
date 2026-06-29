using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAlquilerHerramientas.Models;

namespace SistemaAlquilerHerramientas.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class MoraController : Controller
    {
        private readonly AlquilerHerramientasContext _context;

        public MoraController(AlquilerHerramientasContext context)
        {
            _context = context;
        }

        // GET: Mora
        public async Task<IActionResult> Index()
        {
            var alquilerHerramientasContext = _context.Moras.Include(m => m.IdAlquilerNavigation);
            return View(await alquilerHerramientasContext.ToListAsync());
        }

        // GET: Mora/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mora = await _context.Moras
                .Include(m => m.IdAlquilerNavigation)
                .FirstOrDefaultAsync(m => m.IdMora == id);
            if (mora == null)
            {
                return NotFound();
            }

            return View(mora);
        }

        // GET: Mora/Create
        public IActionResult Create()
        {
            ViewData["IdAlquiler"] = new SelectList(_context.Alquilers, "IdAlquiler", "IdAlquiler");
            return View();
        }

        // POST: Mora/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdMora,IdAlquiler,DiasRetraso,MontoMora,EstadoPago")] Mora mora)
        {
            if (ModelState.IsValid)
            {
                _context.Add(mora);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdAlquiler"] = new SelectList(_context.Alquilers, "IdAlquiler", "IdAlquiler", mora.IdAlquiler);
            return View(mora);
        }

        // GET: Mora/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mora = await _context.Moras.FindAsync(id);
            if (mora == null)
            {
                return NotFound();
            }
            ViewData["IdAlquiler"] = new SelectList(_context.Alquilers, "IdAlquiler", "IdAlquiler", mora.IdAlquiler);
            return View(mora);
        }

        // POST: Mora/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdMora,IdAlquiler,DiasRetraso,MontoMora,EstadoPago")] Mora mora)
        {
            if (id != mora.IdMora)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mora);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MoraExists(mora.IdMora))
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
            ViewData["IdAlquiler"] = new SelectList(_context.Alquilers, "IdAlquiler", "IdAlquiler", mora.IdAlquiler);
            return View(mora);
        }

        // GET: Mora/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mora = await _context.Moras
                .Include(m => m.IdAlquilerNavigation)
                .FirstOrDefaultAsync(m => m.IdMora == id);
            if (mora == null)
            {
                return NotFound();
            }

            return View(mora);
        }

        // POST: Mora/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mora = await _context.Moras.FindAsync(id);
            if (mora != null)
            {
                _context.Moras.Remove(mora);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MoraExists(int id)
        {
            return _context.Moras.Any(e => e.IdMora == id);
        }
    }
}
