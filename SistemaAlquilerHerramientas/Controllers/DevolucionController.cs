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
    public class DevolucionController : Controller
    {
        private readonly AlquilerHerramientasContext _context;

        public DevolucionController(AlquilerHerramientasContext context)
        {
            _context = context;
        }

        // GET: Devolucion
        public async Task<IActionResult> Index()
        {
            var alquilerHerramientasContext = _context.Devolucions.Include(d => d.IdAlquilerNavigation);
            return View(await alquilerHerramientasContext.ToListAsync());
        }

        // GET: Devolucion/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var devolucion = await _context.Devolucions
                .Include(d => d.IdAlquilerNavigation)
                .FirstOrDefaultAsync(m => m.IdDevolucion == id);
            if (devolucion == null)
            {
                return NotFound();
            }

            return View(devolucion);
        }

        // GET: Devolucion/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDevolucionSelectsAsync();
            return View();
        }

        // POST: Devolucion/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdDevolucion,IdAlquiler,FechaDevolucionReal,EstadoRetorno,Observacion,FechaRegistro")] Devolucion devolucion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(devolucion);
                await ActualizarAlquilerPorDevolucionAsync(devolucion);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            await PopulateDevolucionSelectsAsync(devolucion);
            return View(devolucion);
        }

        // GET: Devolucion/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var devolucion = await _context.Devolucions.FindAsync(id);
            if (devolucion == null)
            {
                return NotFound();
            }
            await PopulateDevolucionSelectsAsync(devolucion);
            return View(devolucion);
        }

        // POST: Devolucion/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdDevolucion,IdAlquiler,FechaDevolucionReal,EstadoRetorno,Observacion,FechaRegistro")] Devolucion devolucion)
        {
            if (id != devolucion.IdDevolucion)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(devolucion);
                    await ActualizarAlquilerPorDevolucionAsync(devolucion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DevolucionExists(devolucion.IdDevolucion))
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
            await PopulateDevolucionSelectsAsync(devolucion);
            return View(devolucion);
        }

        // GET: Devolucion/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var devolucion = await _context.Devolucions
                .Include(d => d.IdAlquilerNavigation)
                .FirstOrDefaultAsync(m => m.IdDevolucion == id);
            if (devolucion == null)
            {
                return NotFound();
            }

            return View(devolucion);
        }

        // POST: Devolucion/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var devolucion = await _context.Devolucions.FindAsync(id);
            if (devolucion != null)
            {
                var mora = await _context.Moras
                    .FirstOrDefaultAsync(m => m.IdAlquiler == devolucion.IdAlquiler);

                if (mora != null)
                {
                    _context.Moras.Remove(mora);
                }

                _context.Devolucions.Remove(devolucion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DevolucionExists(int id)
        {
            return _context.Devolucions.Any(e => e.IdDevolucion == id);
        }

        private async Task ActualizarAlquilerPorDevolucionAsync(Devolucion devolucion)
        {
            var alquiler = await _context.Alquilers
                .Include(a => a.IdHerramientaNavigation)
                .FirstOrDefaultAsync(a => a.IdAlquiler == devolucion.IdAlquiler);

            if (alquiler == null)
            {
                return;
            }

            alquiler.EstadoAlquiler = "Cerrado";

            var mora = await _context.Moras
                .FirstOrDefaultAsync(m => m.IdAlquiler == devolucion.IdAlquiler);

            if (devolucion.FechaDevolucionReal <= alquiler.FechaDevolucionPactada)
            {
                if (mora != null)
                {
                    _context.Moras.Remove(mora);
                }

                return;
            }

            var diasRetraso = CalcularDiasRetraso(alquiler.FechaDevolucionPactada, devolucion.FechaDevolucionReal);
            var montoMora = diasRetraso * alquiler.IdHerramientaNavigation.PrecioPorDia;

            if (mora == null)
            {
                _context.Moras.Add(new Mora
                {
                    IdAlquiler = devolucion.IdAlquiler,
                    DiasRetraso = diasRetraso,
                    MontoMora = montoMora,
                    EstadoPago = "Pendiente"
                });
                return;
            }

            mora.DiasRetraso = diasRetraso;
            mora.MontoMora = montoMora;

            if (string.IsNullOrWhiteSpace(mora.EstadoPago))
            {
                mora.EstadoPago = "Pendiente";
            }
        }

        private static int CalcularDiasRetraso(DateTime fechaPactada, DateTime fechaReal)
        {
            var dias = (int)Math.Ceiling((fechaReal - fechaPactada).TotalDays);
            return Math.Max(dias, 1);
        }

        private async Task PopulateDevolucionSelectsAsync(Devolucion? devolucion = null)
        {
            var alquileres = await _context.Alquilers
                .Include(a => a.IdClienteNavigation)
                .Include(a => a.IdHerramientaNavigation)
                .OrderByDescending(a => a.IdAlquiler)
                .Select(a => new
                {
                    a.IdAlquiler,
                    Descripcion = "Alquiler #" + a.IdAlquiler + " - " + a.IdClienteNavigation.Nombres + " " + a.IdClienteNavigation.Apellidos + " - " + a.IdHerramientaNavigation.Nombre
                })
                .ToListAsync();

            ViewData["IdAlquiler"] = new SelectList(alquileres, "IdAlquiler", "Descripcion", devolucion?.IdAlquiler);
        }
    }
}
