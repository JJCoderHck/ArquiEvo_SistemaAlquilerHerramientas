using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAlquilerHerramientas.Models;
using SistemaAlquilerHerramientas.Services;

namespace SistemaAlquilerHerramientas.Controllers
{
    public class ReportesController : Controller
    {
        private readonly AlquilerHerramientasContext _context;
        private readonly PdfReporteService _pdfService;
        private readonly ILogger<ReportesController> _logger;

        // Inyección de dependencias - Entity Framework se inyecta aquí
        public ReportesController(AlquilerHerramientasContext context, PdfReporteService pdfService, ILogger<ReportesController> logger)
        {
            _context = context;
            _pdfService = pdfService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        // EJEMPLO 1: Herramientas Disponibles
        [HttpPost]
        public async Task<IActionResult> DescargarHerramientasDisponibles()
        {
            try
            {
                // Aquí usamos Entity Framework para obtener datos de la BD
                var herramientas = await _context.Herramienta
                    .Include(h => h.IdCategoriaNavigation)      // INNER JOIN con Categoría
                    .Include(h => h.IdProveedorNavigation)      // INNER JOIN con Proveedor
                    .Where(h => h.EstadoHerramienta == "Disponible")  // WHERE
                    .Select(h => new ReporteHerramientaViewModel
                    {
                        IdHerramienta = h.IdHerramienta,
                        Nombre = h.Nombre,
                        Descripcion = h.Descripcion,
                        PrecioPorDia = h.PrecioPorDia,
                        Categoria = h.IdCategoriaNavigation!.IdCategoria.ToString(),
                        Proveedor = h.IdProveedorNavigation!.IdProveedor.ToString(),
                        Estado = h.EstadoHerramienta
                    })
                    .OrderBy(h => h.Nombre)        // ORDER BY
                    .ToListAsync();                 // Ejecuta la query en BD de forma asincrónica

                // Generamos el PDF con los datos obtenidos
                var pdf = _pdfService.GenerarPdfHerramientasDisponibles(herramientas);

                // Retornamos el archivo PDF para descargar
                return File(pdf, "application/pdf", $"Reporte_Herramientas_Disponibles_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF de herramientas disponibles");
                return RedirectToAction("Index");
            }
        }

        // EJEMPLO 2: Alquileres Activos con cálculos
        [HttpPost]
        public async Task<IActionResult> DescargarAlquileresActivos()
        {
            try
            {
                var alquileres = await _context.Alquilers
                    .Include(a => a.IdClienteNavigation)         // Datos del cliente
                    .Include(a => a.IdHerramientaNavigation)     // Datos de la herramienta
                    .Where(a => a.EstadoAlquiler == "Activo")    // Solo activos
                    .Select(a => new ReporteAlquilerViewModel
                    {
                        IdAlquiler = a.IdAlquiler,
                        // Concatenamos nombres y apellidos
                        Cliente = a.IdClienteNavigation!.Nombres + " " + a.IdClienteNavigation!.Apellidos,
                        Herramienta = a.IdHerramientaNavigation!.Nombre,
                        FechaEntrega = a.FechaEntrega,
                        FechaDevolucionPactada = a.FechaDevolucionPactada,
                        MontoEstimado = a.MontoEstimado ?? 0,    // Null coalescing
                        Estado = a.EstadoAlquiler,
                        DiasTranscurridos = (DateTime.Now - a.FechaEntrega).Days  // Calculamos días
                    })
                    .OrderByDescending(a => a.FechaEntrega)
                    .ToListAsync();

                var pdf = _pdfService.GenerarPdfAlquileresActivos(alquileres);
                return File(pdf, "application/pdf", $"Reporte_Alquileres_Activos_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF de alquileres activos");
                return RedirectToAction("Index");
            }
        }

        // EJEMPLO 3: Reservas
        [HttpPost]
        public async Task<IActionResult> DescargarReservas()
        {
            try
            {
                var reservas = await _context.Reservas
                    .Include(r => r.IdClienteNavigation)
                    .Include(r => r.IdHerramientaNavigation)
                    .Select(r => new ReporteReservaViewModel
                    {
                        IdReserva = r.IdReserva,
                        Cliente = r.IdClienteNavigation!.Nombres + " " + r.IdClienteNavigation!.Apellidos,
                        Herramienta = r.IdHerramientaNavigation!.Nombre,
                        FechaInicio = r.FechaInicio,
                        FechaDevolucionEstimada = r.FechaDevolucionEstimada,
                        Estado = r.EstadoReserva,
                        FechaRegistro = r.FechaRegistro ?? DateTime.Now,
                        DiasReserva = (r.FechaDevolucionEstimada - r.FechaInicio).Days
                    })
                    .OrderByDescending(r => r.FechaInicio)
                    .ToListAsync();

                var pdf = _pdfService.GenerarPdfReservas(reservas);
                return File(pdf, "application/pdf", $"Reporte_Reservas_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF de reservas");
                return RedirectToAction("Index");
            }
        }

        // EJEMPLO 4: Devoluciones con JOINs complejos
        [HttpPost]
        public async Task<IActionResult> DescargarDevoluciones()
        {
            try
            {
                var devoluciones = await _context.Devolucions
                    .Include(d => d.IdAlquilerNavigation)                          // Include Alquiler
                    .ThenInclude(a => a!.IdClienteNavigation)                      // ThenInclude Cliente
                    .Include(d => d.IdAlquilerNavigation)
                    .ThenInclude(a => a!.IdHerramientaNavigation)                  // ThenInclude Herramienta
                    .Select(d => new ReporteDevolucionViewModel
                    {
                        IdDevolucion = d.IdDevolucion,
                        IdAlquiler = d.IdAlquiler,
                        // Accedemos a datos relacionados a través de navegación
                        Cliente = d.IdAlquilerNavigation!.IdClienteNavigation!.Nombres + " " +
                                 d.IdAlquilerNavigation!.IdClienteNavigation!.Apellidos,
                        Herramienta = d.IdAlquilerNavigation!.IdHerramientaNavigation!.Nombre,
                        FechaDevolucionReal = d.FechaDevolucionReal,
                        FechaDevolucionPactada = d.IdAlquilerNavigation!.FechaDevolucionPactada,
                        EstadoRetorno = d.EstadoRetorno,
                        Observacion = d.Observacion,
                        DiasRetraso = (d.FechaDevolucionReal - d.IdAlquilerNavigation!.FechaDevolucionPactada).Days
                    })
                    .OrderByDescending(d => d.FechaDevolucionReal)
                    .ToListAsync();

                var pdf = _pdfService.GenerarPdfDevoluciones(devoluciones);
                return File(pdf, "application/pdf", $"Reporte_Devoluciones_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF de devoluciones");
                return RedirectToAction("Index");
            }
        }

        // EJEMPLO 5: Moras
        [HttpPost]
        public async Task<IActionResult> DescargarMoras()
        {
            try
            {
                var moras = await _context.Moras
                    .Include(m => m.IdAlquilerNavigation)
                    .ThenInclude(a => a!.IdClienteNavigation)
                    .Include(m => m.IdAlquilerNavigation)
                    .ThenInclude(a => a!.IdHerramientaNavigation)
                    .Select(m => new ReporteMoraViewModel
                    {
                        IdMora = m.IdMora,
                        IdAlquiler = m.IdAlquiler,
                        Cliente = m.IdAlquilerNavigation!.IdClienteNavigation!.Nombres + " " +
                                 m.IdAlquilerNavigation!.IdClienteNavigation!.Apellidos,
                        Herramienta = m.IdAlquilerNavigation!.IdHerramientaNavigation!.Nombre,
                        DiasRetraso = m.DiasRetraso,
                        MontoMora = m.MontoMora,
                        EstadoPago = m.EstadoPago
                    })
                    .OrderByDescending(m => m.MontoMora)
                    .ToListAsync();

                var pdf = _pdfService.GenerarPdfMoras(moras);
                return File(pdf, "application/pdf", $"Reporte_Moras_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF de moras");
                return RedirectToAction("Index");
            }
        }
    }
}
