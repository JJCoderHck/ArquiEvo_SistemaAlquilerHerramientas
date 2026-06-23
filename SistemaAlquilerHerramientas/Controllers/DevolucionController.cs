using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAlquilerHerramientas.Models;
using SistemaAlquilerHerramientas.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaAlquilerHerramientas.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class DevolucionController : Controller
    {
        private readonly AlquilerHerramientasContext _context;
        private readonly HerramientaEstadoService _estadoService;

        public DevolucionController(AlquilerHerramientasContext context, HerramientaEstadoService estadoService)
        {
            _context = context;
            _estadoService = estadoService;
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
        public IActionResult Create()
        {
            ViewData["IdAlquiler"] = new SelectList(_context.Alquilers, "IdAlquiler", "IdAlquiler");
            return View();
        }

        // POST: Devolucion/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdDevolucion,IdAlquiler,FechaDevolucionReal,EstadoRetorno,Observacion,FechaRegistro")]Devolucion devolucion)
        {
            try
            {
                // Obtener el alquiler relacionado
                var alquiler = await _context.Alquilers.FindAsync(devolucion.IdAlquiler);

                if (alquiler == null)
                {
                    ModelState.AddModelError("IdAlquiler", "Alquiler no encontrado.");
                }
                else if (alquiler.EstadoAlquiler != "Activo")
                {
                    ModelState.AddModelError("IdAlquiler",
                        $"El alquiler no está activo. Estado actual: {alquiler.EstadoAlquiler}");
                }

                // Validar fecha de devolución
                if (devolucion.FechaDevolucionReal.Date < alquiler?.FechaEntrega.Date)
                {
                    ModelState.AddModelError("FechaDevolucionReal",
                        "La fecha de devolución no puede ser anterior a la fecha de entrega.");
                }

                // Validar estado de retorno
                if (string.IsNullOrWhiteSpace(devolucion.EstadoRetorno))
                {
                    ModelState.AddModelError("EstadoRetorno", "Debe especificar el estado de retorno.");
                }

                if (!ModelState.IsValid)
                {
                    ViewData["IdAlquiler"] = new SelectList(_context.Alquilers, "IdAlquiler", "IdAlquiler", devolucion.IdAlquiler);
                    return View(devolucion);
                }

                devolucion.FechaRegistro = DateTime.Now;
                _context.Devolucions.Add(devolucion);

                // Cerrar el alquiler
                alquiler!.EstadoAlquiler = "Cerrado";

                // RN-11 / RN-12: cambiar estado herramienta según condición de retorno
                string nuevoEstado = devolucion.EstadoRetorno == "Con daños"
                    ? HerramientaEstadoService.Estados.Mantenimiento
                    : HerramientaEstadoService.Estados.Disponible;

                _estadoService.CambiarEstado(alquiler.IdHerramienta, nuevoEstado);

                // RN-13: calcular mora si hay retraso
                if (devolucion.FechaDevolucionReal.Date > alquiler.FechaDevolucionPactada.Date)
                {
                    int diasRetraso = (devolucion.FechaDevolucionReal.Date
                                     - alquiler.FechaDevolucionPactada.Date).Days;

                    var herramienta = await _context.Herramienta.FindAsync(alquiler.IdHerramienta);
                    decimal montoMora = diasRetraso * herramienta!.PrecioPorDia;

                    var mora = new Mora
                    {
                        IdAlquiler = alquiler.IdAlquiler,
                        DiasRetraso = diasRetraso,
                        MontoMora = montoMora,
                        EstadoPago = "Pendiente"
                    };
                    _context.Moras.Add(mora);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al registrar la devolución: " + ex.Message);
                ViewData["IdAlquiler"] = new SelectList(_context.Alquilers, "IdAlquiler", "IdAlquiler", devolucion.IdAlquiler);
                return View(devolucion);
            }
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
            ViewData["IdAlquiler"] = new SelectList(_context.Alquilers, "IdAlquiler", "IdAlquiler", devolucion.IdAlquiler);
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
            ViewData["IdAlquiler"] = new SelectList(_context.Alquilers, "IdAlquiler", "IdAlquiler", devolucion.IdAlquiler);
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
                _context.Devolucions.Remove(devolucion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DevolucionExists(int id)
        {
            return _context.Devolucions.Any(e => e.IdDevolucion == id);
        }
    }
}
