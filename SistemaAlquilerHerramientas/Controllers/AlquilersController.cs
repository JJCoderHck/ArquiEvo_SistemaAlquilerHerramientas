using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAlquilerHerramientas.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaAlquilerHerramientas.Controllers
{
    [Authorize(Roles = "Administrador")]
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
        public async Task<IActionResult> Create()
        {
            var alquiler = new Alquiler
            {
                EstadoAlquiler = "Activo"
            };

            await PopulateAlquilerSelectsAsync(alquiler);
            return View(alquiler);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerDatosReserva(int id)
        {
            var reserva = await _context.Reservas
                .Include(r => r.IdHerramientaNavigation)
                .FirstOrDefaultAsync(r => r.IdReserva == id);

            if (reserva == null)
            {
                return NotFound();
            }

            var diasAlquiler = CalcularDiasAlquiler(reserva.FechaInicio, reserva.FechaDevolucionEstimada);
            var montoEstimado = reserva.IdHerramientaNavigation.PrecioPorDia * diasAlquiler;

            return Json(new
            {
                idCliente = reserva.IdCliente,
                idHerramienta = reserva.IdHerramienta,
                fechaEntrega = FormatearFechaParaInput(reserva.FechaInicio),
                fechaDevolucionPactada = FormatearFechaParaInput(reserva.FechaDevolucionEstimada),
                montoEstimado = montoEstimado.ToString("0.00", CultureInfo.InvariantCulture),
                estadoAlquiler = "Activo"
            });
        }

        // POST: Alquilers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdAlquiler,IdCliente,IdHerramienta,IdReserva,FechaEntrega,FechaDevolucionPactada,MontoEstimado,EstadoAlquiler,FechaRegistro")] Alquiler alquiler)
        {
            await AplicarDatosAutomaticosAsync(alquiler);

            if (ModelState.IsValid)
            {
                _context.Add(alquiler);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopulateAlquilerSelectsAsync(alquiler);
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
            await PopulateAlquilerSelectsAsync(alquiler);
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

            await PopulateAlquilerSelectsAsync(alquiler);
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

        private async Task AplicarDatosAutomaticosAsync(Alquiler alquiler)
        {
            alquiler.EstadoAlquiler = "Activo";

            if (alquiler.IdReserva.HasValue)
            {
                var reserva = await _context.Reservas
                    .Include(r => r.IdHerramientaNavigation)
                    .FirstOrDefaultAsync(r => r.IdReserva == alquiler.IdReserva.Value);

                if (reserva == null)
                {
                    ModelState.AddModelError(nameof(alquiler.IdReserva), "La reserva seleccionada no existe.");
                    return;
                }

                alquiler.IdCliente = reserva.IdCliente;
                alquiler.IdHerramienta = reserva.IdHerramienta;
                alquiler.FechaEntrega = reserva.FechaInicio;
                alquiler.FechaDevolucionPactada = reserva.FechaDevolucionEstimada;
                alquiler.MontoEstimado = CalcularMontoEstimado(reserva.IdHerramientaNavigation.PrecioPorDia, alquiler.FechaEntrega, alquiler.FechaDevolucionPactada);
                reserva.EstadoReserva = "Completada";
                return;
            }

            if (alquiler.FechaDevolucionPactada < alquiler.FechaEntrega)
            {
                ModelState.AddModelError(nameof(alquiler.FechaDevolucionPactada), "La fecha pactada de devolucion no puede ser anterior a la fecha de entrega.");
                return;
            }

            var herramienta = await _context.Herramienta.FindAsync(alquiler.IdHerramienta);
            if (herramienta == null)
            {
                ModelState.AddModelError(nameof(alquiler.IdHerramienta), "La herramienta seleccionada no existe.");
                return;
            }

            alquiler.MontoEstimado = CalcularMontoEstimado(herramienta.PrecioPorDia, alquiler.FechaEntrega, alquiler.FechaDevolucionPactada);
        }

        private async Task PopulateAlquilerSelectsAsync(Alquiler? alquiler = null)
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

            var reservas = await _context.Reservas
                .Include(r => r.IdClienteNavigation)
                .Include(r => r.IdHerramientaNavigation)
                .OrderByDescending(r => r.FechaRegistro)
                .ThenByDescending(r => r.IdReserva)
                .Select(r => new
                {
                    r.IdReserva,
                    Descripcion = "Reserva " + r.IdReserva + " - " + r.IdClienteNavigation.Nombres + " " + r.IdClienteNavigation.Apellidos + " - " + r.IdHerramientaNavigation.Nombre
                })
                .ToListAsync();

            ViewData["IdCliente"] = new SelectList(clientes, "IdCliente", "NombreCompleto", alquiler?.IdCliente);
            ViewData["IdHerramienta"] = new SelectList(herramientas, "IdHerramienta", "Nombre", alquiler?.IdHerramienta);
            ViewData["IdReserva"] = new SelectList(reservas, "IdReserva", "Descripcion", alquiler?.IdReserva);
        }

        private static decimal CalcularMontoEstimado(decimal precioPorDia, DateTime fechaEntrega, DateTime fechaDevolucionPactada)
        {
            return precioPorDia * CalcularDiasAlquiler(fechaEntrega, fechaDevolucionPactada);
        }

        private static int CalcularDiasAlquiler(DateTime fechaEntrega, DateTime fechaDevolucionPactada)
        {
            var dias = (int)Math.Ceiling((fechaDevolucionPactada - fechaEntrega).TotalDays);
            return Math.Max(dias, 1);
        }

        private static string FormatearFechaParaInput(DateTime fecha)
        {
            return fecha.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);
        }
    }
}
