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
    public class AlquilersController : Controller
    {
        private readonly AlquilerHerramientasContext _context;

        public AlquilersController(AlquilerHerramientasContext context)
        {
            _context = context;
        }

        // GET: Alquilers
        public async Task<IActionResult> Index()
        {
            var alquilerHerramientasContext = _context.Alquilers.Include(a => a.IdClienteNavigation).Include(a => a.IdHerramientaNavigation).Include(a => a.IdReservaNavigation);
            return View(await alquilerHerramientasContext.ToListAsync());
        }

        // GET: Alquilers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alquiler = await _context.Alquilers
                .Include(a => a.IdClienteNavigation)
                .Include(a => a.IdHerramientaNavigation)
                .Include(a => a.IdReservaNavigation)
                .FirstOrDefaultAsync(m => m.IdAlquiler == id);
            if (alquiler == null)
            {
                return NotFound();
            }

            return View(alquiler);
        }

        // GET: Alquilers/Create
        public IActionResult Create()
        {
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "IdCliente", "IdCliente");
            ViewData["IdHerramienta"] = new SelectList(_context.Herramienta, "IdHerramienta", "IdHerramienta");
            ViewData["IdReserva"] = new SelectList(_context.Reservas, "IdReserva", "IdReserva");
            return View();
        }

        // POST: Alquilers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdAlquiler,IdCliente,IdHerramienta,IdReserva,FechaEntrega,FechaDevolucionPactada,MontoEstimado,EstadoAlquiler,FechaRegistro")] Alquiler alquiler)
        {
            if (ModelState.IsValid)
            {
                _context.Add(alquiler);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "IdCliente", "IdCliente", alquiler.IdCliente);
            ViewData["IdHerramienta"] = new SelectList(_context.Herramienta, "IdHerramienta", "IdHerramienta", alquiler.IdHerramienta);
            ViewData["IdReserva"] = new SelectList(_context.Reservas, "IdReserva", "IdReserva", alquiler.IdReserva);
            return View(alquiler);
        }

        // GET: Alquilers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alquiler = await _context.Alquilers.FindAsync(id);
            if (alquiler == null)
            {
                return NotFound();
            }
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "IdCliente", "IdCliente", alquiler.IdCliente);
            ViewData["IdHerramienta"] = new SelectList(_context.Herramienta, "IdHerramienta", "IdHerramienta", alquiler.IdHerramienta);
            ViewData["IdReserva"] = new SelectList(_context.Reservas, "IdReserva", "IdReserva", alquiler.IdReserva);
            return View(alquiler);
        }

        // POST: Alquilers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdAlquiler,IdCliente,IdHerramienta,IdReserva,FechaEntrega,FechaDevolucionPactada,MontoEstimado,EstadoAlquiler,FechaRegistro")] Alquiler alquiler)
        {
            if (id != alquiler.IdAlquiler)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(alquiler);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AlquilerExists(alquiler.IdAlquiler))
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
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "IdCliente", "IdCliente", alquiler.IdCliente);
            ViewData["IdHerramienta"] = new SelectList(_context.Herramienta, "IdHerramienta", "IdHerramienta", alquiler.IdHerramienta);
            ViewData["IdReserva"] = new SelectList(_context.Reservas, "IdReserva", "IdReserva", alquiler.IdReserva);
            return View(alquiler);
        }

        // GET: Alquilers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var alquiler = await _context.Alquilers
                .Include(a => a.IdClienteNavigation)
                .Include(a => a.IdHerramientaNavigation)
                .Include(a => a.IdReservaNavigation)
                .FirstOrDefaultAsync(m => m.IdAlquiler == id);
            if (alquiler == null)
            {
                return NotFound();
            }

            return View(alquiler);
        }

        // POST: Alquilers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var alquiler = await _context.Alquilers.FindAsync(id);
            if (alquiler != null)
            {
                _context.Alquilers.Remove(alquiler);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AlquilerExists(int id)
        {
            return _context.Alquilers.Any(e => e.IdAlquiler == id);
        }
    }
}
