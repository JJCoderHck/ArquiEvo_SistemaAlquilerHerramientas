using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAlquilerHerramientas.Models;

namespace SistemaAlquilerHerramientas.Controllers
{
    public class ClientesController : Controller
    {
        private readonly AlquilerHerramientasContext _context;
        private readonly ILogger<ClientesController> _logger;

        public ClientesController(AlquilerHerramientasContext context, ILogger<ClientesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Clientes
        public async Task<IActionResult> Index()
        {
            return View(await _context.Clientes.ToListAsync());
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.IdCliente == id);
            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // GET: Clientes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCliente,Nombres,Apellidos,Dni,Telefono,Correo,Direccion,Estado")] Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Validar Nombres no vacío
                    if (string.IsNullOrWhiteSpace(cliente.Nombres))
                    {
                        ModelState.AddModelError("Nombres", "El nombre es requerido");
                        return View(cliente);
                    }

                    // Validar Apellidos no vacío
                    if (string.IsNullOrWhiteSpace(cliente.Apellidos))
                    {
                        ModelState.AddModelError("Apellidos", "El apellido es requerido");
                        return View(cliente);
                    }

                    // Validar DNI (8 dígitos exactos)
                    if (string.IsNullOrWhiteSpace(cliente.Dni) || !Regex.IsMatch(cliente.Dni, @"^\d{8}$"))
                    {
                        ModelState.AddModelError("Dni", "El DNI debe tener 8 dígitos");
                        return View(cliente);
                    }

                    // Verificar DNI duplicado
                    var dniExistente = await _context.Clientes
                        .FirstOrDefaultAsync(c => c.Dni == cliente.Dni);

                    if (dniExistente != null)
                    {
                        ModelState.AddModelError("Dni", "Este DNI ya está registrado en el sistema");
                        return View(cliente);
                    }

                    // Validar Teléfono si está presente (9 dígitos)
                    if (!string.IsNullOrWhiteSpace(cliente.Telefono) && !Regex.IsMatch(cliente.Telefono, @"^\d{9}$"))
                    {
                        ModelState.AddModelError("Telefono", "El teléfono debe tener 9 dígitos");
                        return View(cliente);
                    }

                    // Validar Correo si está presente
                    if (!string.IsNullOrWhiteSpace(cliente.Correo) && !Regex.IsMatch(cliente.Correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    {
                        ModelState.AddModelError("Correo", "El correo es inválido");
                        return View(cliente);
                    }

                    // Asignar estado por defecto si no viene
                    cliente.Estado = cliente.Estado ?? "Activo";

                    _context.Add(cliente);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Cliente creado: {cliente.Nombres} {cliente.Apellidos} (DNI: {cliente.Dni})");
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error de base de datos al crear cliente");
                    ModelState.AddModelError("", "Error al guardar en la base de datos");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inesperado al crear cliente");
                    ModelState.AddModelError("", "Error inesperado al crear el cliente");
                }
            }

            return View(cliente);
        }

        // GET: Clientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }
            return View(cliente);
        }

        // POST: Clientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCliente,Nombres,Apellidos,Dni,Telefono,Correo,Direccion,Estado")] Cliente cliente)
        {
            if (id != cliente.IdCliente)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Validar Nombres no vacío
                    if (string.IsNullOrWhiteSpace(cliente.Nombres))
                    {
                        ModelState.AddModelError("Nombres", "El nombre es requerido");
                        return View(cliente);
                    }

                    // Validar Apellidos no vacío
                    if (string.IsNullOrWhiteSpace(cliente.Apellidos))
                    {
                        ModelState.AddModelError("Apellidos", "El apellido es requerido");
                        return View(cliente);
                    }

                    // Validar DNI (8 dígitos exactos)
                    if (string.IsNullOrWhiteSpace(cliente.Dni) || !Regex.IsMatch(cliente.Dni, @"^\d{8}$"))
                    {
                        ModelState.AddModelError("Dni", "El DNI debe tener 8 dígitos");
                        return View(cliente);
                    }

                    // Verificar DNI duplicado (excluyendo el registro actual)
                    var dniExistente = await _context.Clientes
                        .FirstOrDefaultAsync(c => c.Dni == cliente.Dni && c.IdCliente != id);

                    if (dniExistente != null)
                    {
                        ModelState.AddModelError("Dni", "Este DNI ya está registrado en el sistema");
                        return View(cliente);
                    }

                    // Validar Teléfono si está presente (9 dígitos)
                    if (!string.IsNullOrWhiteSpace(cliente.Telefono) && !Regex.IsMatch(cliente.Telefono, @"^\d{9}$"))
                    {
                        ModelState.AddModelError("Telefono", "El teléfono debe tener 9 dígitos");
                        return View(cliente);
                    }

                    // Validar Correo si está presente
                    if (!string.IsNullOrWhiteSpace(cliente.Correo) && !Regex.IsMatch(cliente.Correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    {
                        ModelState.AddModelError("Correo", "El correo es inválido");
                        return View(cliente);
                    }

                    _context.Update(cliente);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Cliente actualizado: {cliente.Nombres} {cliente.Apellidos} (DNI: {cliente.Dni})");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(cliente.IdCliente))
                    {
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError("Error de concurrencia al editar cliente");
                        ModelState.AddModelError("", "El cliente fue modificado por otro usuario");
                        return View(cliente);
                    }
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error de base de datos al editar cliente");
                    ModelState.AddModelError("", "Error al guardar en la base de datos");
                    return View(cliente);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inesperado al editar cliente");
                    ModelState.AddModelError("", "Error inesperado al editar el cliente");
                    return View(cliente);
                }

                return RedirectToAction(nameof(Index));
            }

            return View(cliente);
        }

        // GET: Clientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.IdCliente == id);
            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var cliente = await _context.Clientes.FindAsync(id);
                if (cliente != null)
                {
                    // Verificar si tiene alquileres asociados
                    var tieneAlquileres = await _context.Alquilers
                        .AnyAsync(a => a.IdCliente == id);

                    if (tieneAlquileres)
                    {
                        _logger.LogWarning($"Intento de eliminar cliente con alquileres: {cliente.Nombres}");
                        TempData["ErrorMessage"] = "No se puede eliminar este cliente porque tiene alquileres asociados";
                        return RedirectToAction(nameof(Index));
                    }

                    // Verificar si tiene reservas asociadas
                    var tieneReservas = await _context.Reservas
                        .AnyAsync(r => r.IdCliente == id);

                    if (tieneReservas)
                    {
                        _logger.LogWarning($"Intento de eliminar cliente con reservas: {cliente.Nombres}");
                        TempData["ErrorMessage"] = "No se puede eliminar este cliente porque tiene reservas asociadas";
                        return RedirectToAction(nameof(Index));
                    }

                    _context.Clientes.Remove(cliente);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Cliente eliminado: {cliente.Nombres} {cliente.Apellidos}");
                }

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error de base de datos al eliminar cliente");
                TempData["ErrorMessage"] = "Error al eliminar el cliente";
                return RedirectToAction(nameof(Delete), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al eliminar cliente");
                TempData["ErrorMessage"] = "Error inesperado al eliminar el cliente";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.IdCliente == id);
        }
    }
}