using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAlquilerHerramientas.Models;
using System.Security.Claims;

namespace SistemaAlquilerHerramientas.Controllers
{
    [Authorize(Roles = "Administrador,Cliente")]
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
            var moras = _context.Moras
                .Include(m => m.IdAlquilerNavigation)
                .ThenInclude(a => a.IdClienteNavigation)
                .Include(m => m.IdAlquilerNavigation)
                .ThenInclude(a => a.IdHerramientaNavigation)
                .AsQueryable();

            if (!User.IsInRole("Administrador"))
            {
                var cliente = await GetCurrentClienteAsync();
                moras = cliente == null
                    ? moras.Where(m => false)
                    : moras.Where(m => m.IdAlquilerNavigation.IdCliente == cliente.IdCliente);
            }

            return View(await moras.OrderByDescending(m => m.IdMora).ToListAsync());
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
                .ThenInclude(a => a.IdClienteNavigation)
                .Include(m => m.IdAlquilerNavigation)
                .ThenInclude(a => a.IdHerramientaNavigation)
                .FirstOrDefaultAsync(m => m.IdMora == id);
            if (mora == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Administrador") && !await MoraPerteneceAlClienteActualAsync(mora))
                return Forbid();

            return View(mora);
        }

        // GET: Mora/Pagar/5
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Pagar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mora = await FindMoraForPaymentAsync(id.Value);
            if (mora == null)
            {
                return NotFound();
            }

            if (!await MoraPerteneceAlClienteActualAsync(mora))
                return Forbid();

            if (string.Equals(mora.EstadoPago, "Pagado", StringComparison.OrdinalIgnoreCase))
            {
                TempData["SuccessMessage"] = "La mora ya se encuentra pagada.";
                return RedirectToAction(nameof(Index));
            }

            return View(mora);
        }

        // POST: Mora/Pagar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> Pagar(int id, string titularTarjeta, string numeroTarjeta, string fechaVencimiento, string cvv)
        {
            var mora = await FindMoraForPaymentAsync(id);
            if (mora == null)
            {
                return NotFound();
            }

            if (!await MoraPerteneceAlClienteActualAsync(mora))
                return Forbid();

            ValidarPago(titularTarjeta, numeroTarjeta, fechaVencimiento, cvv);

            if (!ModelState.IsValid)
            {
                return View(mora);
            }

            mora.EstadoPago = "Pagado";
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Pago registrado correctamente. La mora ahora figura como pagada.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Mora/Create
        [Authorize(Roles = "Administrador")]
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
        [Authorize(Roles = "Administrador")]
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
        [Authorize(Roles = "Administrador")]
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
        [Authorize(Roles = "Administrador")]
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
        [Authorize(Roles = "Administrador")]
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
        [Authorize(Roles = "Administrador")]
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

        private async Task<Mora?> FindMoraForPaymentAsync(int id)
        {
            return await _context.Moras
                .Include(m => m.IdAlquilerNavigation)
                .ThenInclude(a => a.IdClienteNavigation)
                .Include(m => m.IdAlquilerNavigation)
                .ThenInclude(a => a.IdHerramientaNavigation)
                .FirstOrDefaultAsync(m => m.IdMora == id);
        }

        private async Task<Cliente?> GetCurrentClienteAsync()
        {
            var correo = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrWhiteSpace(correo))
                return null;

            return await _context.Clientes
                .FirstOrDefaultAsync(c => c.Correo != null && c.Correo == correo);
        }

        private async Task<bool> MoraPerteneceAlClienteActualAsync(Mora mora)
        {
            var cliente = await GetCurrentClienteAsync();
            return cliente != null && mora.IdAlquilerNavigation.IdCliente == cliente.IdCliente;
        }

        private void ValidarPago(string titularTarjeta, string numeroTarjeta, string fechaVencimiento, string cvv)
        {
            if (string.IsNullOrWhiteSpace(titularTarjeta))
                ModelState.AddModelError(nameof(titularTarjeta), "Ingresa el titular de la tarjeta.");

            var numeroNormalizado = new string((numeroTarjeta ?? string.Empty).Where(char.IsDigit).ToArray());
            if (numeroNormalizado.Length < 13 || numeroNormalizado.Length > 19)
                ModelState.AddModelError(nameof(numeroTarjeta), "Ingresa un numero de tarjeta valido.");

            if (string.IsNullOrWhiteSpace(fechaVencimiento))
                ModelState.AddModelError(nameof(fechaVencimiento), "Ingresa la fecha de vencimiento.");

            var cvvNormalizado = new string((cvv ?? string.Empty).Where(char.IsDigit).ToArray());
            if (cvvNormalizado.Length < 3 || cvvNormalizado.Length > 4)
                ModelState.AddModelError(nameof(cvv), "Ingresa un CVV valido.");
        }
    }
}
