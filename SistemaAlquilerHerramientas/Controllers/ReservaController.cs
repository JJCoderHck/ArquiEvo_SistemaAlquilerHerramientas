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
    [Authorize(Roles = "Administrador,Cliente")]
    public class ReservaController : Controller
    {
        private readonly AlquilerHerramientasContext _context;
        private readonly HerramientaEstadoService _estadoService;

        public ReservaController(AlquilerHerramientasContext context, HerramientaEstadoService estadoService)
        {
            _context = context;
            _estadoService = estadoService;
        }

        // GET: Reserva
        public async Task<IActionResult> Index()
        {
            var alquilerHerramientasContext = _context.Reservas.Include(r => r.IdClienteNavigation).Include(r => r.IdHerramientaNavigation);
            return View(await alquilerHerramientasContext.ToListAsync());
        }

        // GET: Reserva/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.IdClienteNavigation)
                .Include(r => r.IdHerramientaNavigation)
                .FirstOrDefaultAsync(m => m.IdReserva == id);
            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }

        // GET: Reserva/Create
        public IActionResult Create()
        {
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "IdCliente", "IdCliente");
            ViewData["IdHerramienta"] = new SelectList(_context.Herramienta, "IdHerramienta", "IdHerramienta");
            return View();
        }

        // POST: Reserva/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdReserva,IdCliente,IdHerramienta,FechaInicio,FechaDevolucionEstimada,EstadoReserva,FechaRegistro")]Reserva reserva)
        {
            try
            {
                // Validar que cliente existe
                var cliente = await _context.Clientes.FindAsync(reserva.IdCliente);
                if (cliente == null)
                {
                    ModelState.AddModelError("IdCliente", "Cliente no válido.");
                }

                // Validar que herramienta existe
                var herramienta = await _context.Herramienta.FindAsync(reserva.IdHerramienta);
                if (herramienta == null)
                {
                    ModelState.AddModelError("IdHerramienta", "Herramienta no válida.");
                }

                // Validar que la herramienta esté disponible (RN-06)
                if (herramienta != null && !_estadoService.EstaDisponible(reserva.IdHerramienta))
                {
                    ModelState.AddModelError("IdHerramienta",
                        "La herramienta no está disponible para reserva. Estado actual: " + herramienta.EstadoHerramienta);
                }

                // Validar fechas (RN-10)
                if (reserva.FechaDevolucionEstimada <= reserva.FechaInicio)
                {
                    ModelState.AddModelError("FechaDevolucionEstimada",
                        "La fecha de devolución debe ser posterior a la fecha de inicio.");
                }

                // Validar que no sea fecha pasada
                if (reserva.FechaInicio < DateTime.Today)
                {
                    ModelState.AddModelError("FechaInicio",
                        "La fecha de inicio no puede ser en el pasado.");
                }

                // Si hay errores, retornar la vista con los errores
                if (!ModelState.IsValid)
                {
                    ViewData["IdCliente"] = new SelectList(_context.Clientes, "IdCliente", "Nombres", reserva.IdCliente);
                    ViewData["IdHerramienta"] = new SelectList(_context.Herramienta, "IdHerramienta", "Nombre", reserva.IdHerramienta);
                    return View(reserva);
                }

                // Si todo es válido, guardar la reserva
                reserva.EstadoReserva = "Confirmada";
                reserva.FechaRegistro = DateTime.Now;
                _context.Reservas.Add(reserva);
                await _context.SaveChangesAsync();

                // RN-08: cambiar estado herramienta → Reservada
                _estadoService.CambiarEstado(reserva.IdHerramienta,
                    HerramientaEstadoService.Estados.Reservada);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al guardar la reserva: " + ex.Message);
                ViewData["IdCliente"] = new SelectList(_context.Clientes, "IdCliente", "Nombres", reserva.IdCliente);
                ViewData["IdHerramienta"] = new SelectList(_context.Herramienta, "IdHerramienta", "Nombre", reserva.IdHerramienta);
                return View(reserva);
            }
        }

        // GET: Reserva/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
            {
                return NotFound();
            }
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "IdCliente", "IdCliente", reserva.IdCliente);
            ViewData["IdHerramienta"] = new SelectList(_context.Herramienta, "IdHerramienta", "IdHerramienta", reserva.IdHerramienta);
            return View(reserva);
        }

        // POST: Reserva/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdReserva,IdCliente,IdHerramienta,FechaInicio,FechaDevolucionEstimada,EstadoReserva,FechaRegistro")] Reserva reserva)
        {
            if (id != reserva.IdReserva)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reserva);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservaExists(reserva.IdReserva))
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
            ViewData["IdCliente"] = new SelectList(_context.Clientes, "IdCliente", "IdCliente", reserva.IdCliente);
            ViewData["IdHerramienta"] = new SelectList(_context.Herramienta, "IdHerramienta", "IdHerramienta", reserva.IdHerramienta);
            return View(reserva);
        }

        // GET: Reserva/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.IdClienteNavigation)
                .Include(r => r.IdHerramientaNavigation)
                .FirstOrDefaultAsync(m => m.IdReserva == id);
            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }

        // POST: Reserva/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva != null)
            {
                _context.Reservas.Remove(reserva);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservaExists(int id)
        {
            return _context.Reservas.Any(e => e.IdReserva == id);
        }
    }
}
