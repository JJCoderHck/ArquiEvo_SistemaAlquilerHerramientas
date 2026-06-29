using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAlquilerHerramientas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SistemaAlquilerHerramientas.Controllers
{
    [Authorize(Roles = "Administrador,Cliente")]
    public class ReservaController : Controller
    {
        private readonly AlquilerHerramientasContext _context;

        public ReservaController(AlquilerHerramientasContext context)
        {
            _context = context;
        }

        // GET: Reserva
        public async Task<IActionResult> Index()
        {
            var reservas = _context.Reservas
                .Include(r => r.IdClienteNavigation)
                .Include(r => r.IdHerramientaNavigation)
                .AsQueryable();

            if (!User.IsInRole("Administrador"))
            {
                var cliente = await GetCurrentClienteAsync();
                reservas = cliente == null
                    ? reservas.Where(r => false)
                    : reservas.Where(r => r.IdCliente == cliente.IdCliente);
            }

            return View(await reservas.ToListAsync());
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

            if (!User.IsInRole("Administrador") && !await ReservaPerteneceAlClienteActualAsync(reserva.IdCliente))
                return Forbid();

            return View(reserva);
        }

        // GET: Reserva/Create
        public async Task<IActionResult> Create()
        {
            if (!User.IsInRole("Administrador"))
            {
                var cliente = await GetCurrentClienteAsync();
                if (cliente == null)
                {
                    TempData["ErrorMessage"] = "No se encontro un cliente vinculado a tu usuario. Completa el registro de cliente antes de reservar.";
                    return RedirectToAction(nameof(Index));
                }

                ViewData["IdClienteActual"] = cliente.IdCliente;
            }

            await PopulateReservaSelectsAsync();
            return View();
        }

        // POST: Reserva/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdReserva,IdCliente,IdHerramienta,FechaInicio,FechaDevolucionEstimada,EstadoReserva,FechaRegistro")] Reserva reserva)
        {
            if (!User.IsInRole("Administrador"))
            {
                var cliente = await GetCurrentClienteAsync();
                if (cliente == null)
                {
                    ModelState.AddModelError(string.Empty, "No se encontro un cliente vinculado a tu usuario.");
                }
                else
                {
                    reserva.IdCliente = cliente.IdCliente;
                    reserva.EstadoReserva = "Pendiente";
                }
            }

            if (string.IsNullOrWhiteSpace(reserva.EstadoReserva))
                reserva.EstadoReserva = "Pendiente";

            if (reserva.FechaDevolucionEstimada < reserva.FechaInicio)
                ModelState.AddModelError(nameof(reserva.FechaDevolucionEstimada), "La fecha estimada de devolucion no puede ser anterior a la fecha de inicio.");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Reservas.Add(reserva);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Reserva realizada correctamente.";
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError(string.Empty, $"No se pudo guardar la reserva: {ex.GetBaseException().Message}");
                    await PopulateReservaSelectsAsync(reserva);
                    return View(reserva);
                }

                return RedirectToAction(nameof(Index));
            }

            await PopulateReservaSelectsAsync(reserva);
            return View(reserva);
        }

        // GET: Reserva/Edit/5
        [Authorize(Roles = "Administrador")]
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
        [Authorize(Roles = "Administrador")]
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
        [Authorize(Roles = "Administrador")]
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

            await PopulateReservaDeleteInfoAsync(reserva.IdReserva);
            return View(reserva);
        }

        // POST: Reserva/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Alquilers)
                .FirstOrDefaultAsync(r => r.IdReserva == id);

            if (reserva == null)
            {
                return NotFound();
            }

            if (reserva.Alquilers.Any())
            {
                TempData["ErrorMessage"] = "No se puede eliminar la reserva porque tiene un alquiler asociado. Primero elimina el alquiler, o usa la opcion para eliminar el alquiler asociado y la reserva.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteWithAlquileres(int id)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Alquilers)
                .FirstOrDefaultAsync(r => r.IdReserva == id);

            if (reserva != null)
            {
                var alquilerIds = reserva.Alquilers
                    .Select(a => a.IdAlquiler)
                    .ToList();

                if (alquilerIds.Count > 0)
                {
                    var devoluciones = await _context.Devolucions
                        .Where(d => alquilerIds.Contains(d.IdAlquiler))
                        .ToListAsync();

                    var moras = await _context.Moras
                        .Where(m => alquilerIds.Contains(m.IdAlquiler))
                        .ToListAsync();

                    _context.Devolucions.RemoveRange(devoluciones);
                    _context.Moras.RemoveRange(moras);
                    _context.Alquilers.RemoveRange(reserva.Alquilers);
                }

                _context.Reservas.Remove(reserva);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ReservaExists(int id)
        {
            return _context.Reservas.Any(e => e.IdReserva == id);
        }

        private async Task<Cliente?> GetCurrentClienteAsync()
        {
            var correo = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrWhiteSpace(correo))
                return null;

            return await _context.Clientes
                .FirstOrDefaultAsync(c => c.Correo != null && c.Correo == correo);
        }

        private async Task<bool> ReservaPerteneceAlClienteActualAsync(int idCliente)
        {
            var cliente = await GetCurrentClienteAsync();
            return cliente != null && cliente.IdCliente == idCliente;
        }

        private async Task PopulateReservaSelectsAsync(Reserva? reserva = null)
        {
            var clientes = await _context.Clientes
                .OrderBy(c => c.Nombres)
                .ThenBy(c => c.Apellidos)
                .Select(c => new
                {
                    c.IdCliente,
                    NombreCompleto = c.Nombres + " " + c.Apellidos
                })
                .ToListAsync();

            var herramientas = await _context.Herramienta
                .OrderBy(h => h.Nombre)
                .Select(h => new
                {
                    h.IdHerramienta,
                    h.Nombre
                })
                .ToListAsync();

            ViewData["IdCliente"] = new SelectList(clientes, "IdCliente", "NombreCompleto", reserva?.IdCliente);
            ViewData["IdHerramienta"] = new SelectList(herramientas, "IdHerramienta", "Nombre", reserva?.IdHerramienta);
        }

        private async Task PopulateReservaDeleteInfoAsync(int idReserva)
        {
            var alquileresAsociados = await _context.Alquilers
                .Include(a => a.IdHerramientaNavigation)
                .Where(a => a.IdReserva == idReserva)
                .OrderBy(a => a.IdAlquiler)
                .ToListAsync();

            ViewData["AlquileresAsociados"] = alquileresAsociados;
        }
    }
}
