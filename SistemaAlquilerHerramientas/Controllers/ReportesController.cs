using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAlquilerHerramientas.Models;

namespace SistemaAlquilerHerramientas.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ReportesController : Controller
    {
        private static readonly CultureInfo ReportCulture = CultureInfo.GetCultureInfo("es-PE");
        private readonly AlquilerHerramientasContext _context;

        public ReportesController(AlquilerHerramientasContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> HerramientasDisponibles()
        {
            var herramientas = await _context.Herramienta
                .AsNoTracking()
                .Include(h => h.IdCategoriaNavigation)
                .Include(h => h.IdProveedorNavigation)
                .Where(h => h.EstadoHerramienta == null || h.EstadoHerramienta.Contains("Disponible"))
                .OrderBy(h => h.Nombre)
                .ToListAsync();

            var rows = herramientas
                .Select(h => new[]
                {
                    h.IdHerramienta.ToString(),
                    h.Nombre,
                    h.IdCategoriaNavigation.NombreCategoria,
                    h.IdProveedorNavigation.RazonSocial,
                    Money(h.PrecioPorDia),
                    h.EstadoHerramienta ?? "Disponible"
                })
                .ToList<IReadOnlyList<string>>();

            var columns = new[]
            {
                new PdfColumn("ID", 42),
                new PdfColumn("Herramienta", 140),
                new PdfColumn("Categoria", 106),
                new PdfColumn("Proveedor", 122),
                new PdfColumn("Precio Dia", 72),
                new PdfColumn("Estado", 65)
            };

            var summary = $"Total de herramientas disponibles: {herramientas.Count}";
            return Pdf("REPORTE DE HERRAMIENTAS DISPONIBLES", columns, rows, summary, "Reporte_Herramientas_Disponibles.pdf");
        }

        public async Task<IActionResult> AlquileresActivos()
        {
            var alquileres = await _context.Alquilers
                .AsNoTracking()
                .Include(a => a.IdClienteNavigation)
                .Include(a => a.IdHerramientaNavigation)
                .Where(a => a.EstadoAlquiler == null
                    || (a.EstadoAlquiler != "Devuelto"
                        && a.EstadoAlquiler != "Finalizado"
                        && a.EstadoAlquiler != "Cerrado"))
                .OrderByDescending(a => a.FechaEntrega)
                .ToListAsync();

            var rows = alquileres
                .Select(a => new[]
                {
                    a.IdAlquiler.ToString(),
                    FullName(a.IdClienteNavigation),
                    a.IdHerramientaNavigation.Nombre,
                    Date(a.FechaEntrega),
                    Date(a.FechaDevolucionPactada),
                    Money(a.MontoEstimado),
                    a.EstadoAlquiler ?? "Activo"
                })
                .ToList<IReadOnlyList<string>>();

            var columns = new[]
            {
                new PdfColumn("ID Alq", 50),
                new PdfColumn("Cliente", 128),
                new PdfColumn("Herramienta", 120),
                new PdfColumn("Entrega", 68),
                new PdfColumn("Devolucion", 76),
                new PdfColumn("Monto", 62),
                new PdfColumn("Estado", 55)
            };

            var total = alquileres.Sum(a => a.MontoEstimado ?? 0);
            var summary = $"Total de alquileres activos: {alquileres.Count} | Monto estimado: {Money(total)}";
            return Pdf("REPORTE DE ALQUILERES ACTIVOS", columns, rows, summary, "Reporte_Alquileres_Activos.pdf");
        }

        public async Task<IActionResult> Reservas()
        {
            var reservas = await _context.Reservas
                .AsNoTracking()
                .Include(r => r.IdClienteNavigation)
                .Include(r => r.IdHerramientaNavigation)
                .OrderByDescending(r => r.FechaRegistro)
                .ToListAsync();

            var rows = reservas
                .Select(r => new[]
                {
                    r.IdReserva.ToString(),
                    FullName(r.IdClienteNavigation),
                    r.IdHerramientaNavigation.Nombre,
                    Date(r.FechaInicio),
                    Date(r.FechaDevolucionEstimada),
                    r.EstadoReserva ?? "Pendiente"
                })
                .ToList<IReadOnlyList<string>>();

            var columns = new[]
            {
                new PdfColumn("ID Reserva", 70),
                new PdfColumn("Cliente", 142),
                new PdfColumn("Herramienta", 138),
                new PdfColumn("Inicio", 72),
                new PdfColumn("Devolucion", 82),
                new PdfColumn("Estado", 55)
            };

            var pendientes = reservas.Count(r => string.IsNullOrWhiteSpace(r.EstadoReserva)
                || r.EstadoReserva.Contains("Pendiente", StringComparison.OrdinalIgnoreCase));
            var summary = $"Total de reservas: {reservas.Count} | Pendientes: {pendientes}";
            return Pdf("REPORTE DE RESERVAS", columns, rows, summary, "Reporte_Reservas.pdf");
        }

        public async Task<IActionResult> Devoluciones()
        {
            var devoluciones = await _context.Devolucions
                .AsNoTracking()
                .Include(d => d.IdAlquilerNavigation)
                .ThenInclude(a => a.IdClienteNavigation)
                .Include(d => d.IdAlquilerNavigation)
                .ThenInclude(a => a.IdHerramientaNavigation)
                .OrderByDescending(d => d.FechaDevolucionReal)
                .ToListAsync();

            var rows = devoluciones
                .Select(d => new[]
                {
                    d.IdDevolucion.ToString(),
                    d.IdAlquiler.ToString(),
                    FullName(d.IdAlquilerNavigation.IdClienteNavigation),
                    d.IdAlquilerNavigation.IdHerramientaNavigation.Nombre,
                    Date(d.FechaDevolucionReal),
                    d.EstadoRetorno ?? "Sin estado"
                })
                .ToList<IReadOnlyList<string>>();

            var columns = new[]
            {
                new PdfColumn("ID Dev", 50),
                new PdfColumn("ID Alq", 50),
                new PdfColumn("Cliente", 130),
                new PdfColumn("Herramienta", 132),
                new PdfColumn("Fecha", 76),
                new PdfColumn("Estado", 88)
            };

            var summary = $"Total de devoluciones: {devoluciones.Count}";
            return Pdf("REPORTE DE DEVOLUCIONES", columns, rows, summary, "Reporte_Devoluciones.pdf");
        }

        public async Task<IActionResult> Moras()
        {
            var moras = await _context.Moras
                .AsNoTracking()
                .Include(m => m.IdAlquilerNavigation)
                .ThenInclude(a => a.IdClienteNavigation)
                .Include(m => m.IdAlquilerNavigation)
                .ThenInclude(a => a.IdHerramientaNavigation)
                .OrderByDescending(m => m.IdMora)
                .ToListAsync();

            var rows = moras
                .Select(m => new[]
                {
                    m.IdMora.ToString(),
                    m.IdAlquiler.ToString(),
                    FullName(m.IdAlquilerNavigation.IdClienteNavigation),
                    m.IdAlquilerNavigation.IdHerramientaNavigation.Nombre,
                    m.DiasRetraso.ToString(),
                    Money(m.MontoMora),
                    m.EstadoPago ?? "Pendiente"
                })
                .ToList<IReadOnlyList<string>>();

            var columns = new[]
            {
                new PdfColumn("ID Mora", 57),
                new PdfColumn("ID Alq", 56),
                new PdfColumn("Cliente", 113),
                new PdfColumn("Herramienta", 113),
                new PdfColumn("Dias Retraso", 74),
                new PdfColumn("Monto Mora", 74),
                new PdfColumn("Estado Pago", 73)
            };

            var pendientes = moras.Count(m => string.IsNullOrWhiteSpace(m.EstadoPago)
                || m.EstadoPago.Contains("Pendiente", StringComparison.OrdinalIgnoreCase));
            var total = moras.Sum(m => m.MontoMora);
            var summary = $"Total de moras: {moras.Count} | Pendientes: {pendientes} | Monto total: {Money(total)}";
            return Pdf("REPORTE DE MORAS", columns, rows, summary, "Reporte_Moras.pdf");
        }

        private FileContentResult Pdf(
            string title,
            IReadOnlyList<PdfColumn> columns,
            IReadOnlyList<IReadOnlyList<string>> rows,
            string summary,
            string fileName)
        {
            var bytes = BuildTablePdf(title, columns, rows, summary);
            return File(bytes, "application/pdf", fileName);
        }

        private static byte[] BuildTablePdf(
            string title,
            IReadOnlyList<PdfColumn> columns,
            IReadOnlyList<IReadOnlyList<string>> rows,
            string summary)
        {
            var pages = BuildPages(title, columns, rows, summary);
            var objectCount = 2 + pages.Count * 2 + 2;
            var normalFontObjectNumber = objectCount - 1;
            var boldFontObjectNumber = objectCount;
            var objects = new string[objectCount + 1];

            objects[1] = "<< /Type /Catalog /Pages 2 0 R >>";

            var kids = new StringBuilder();
            for (var i = 0; i < pages.Count; i++)
            {
                var pageObjectNumber = 3 + i * 2;
                var contentObjectNumber = pageObjectNumber + 1;
                kids.Append($"{pageObjectNumber} 0 R ");

                var content = pages[i];
                objects[pageObjectNumber] = $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 {normalFontObjectNumber} 0 R /F2 {boldFontObjectNumber} 0 R >> >> /Contents {contentObjectNumber} 0 R >>";
                objects[contentObjectNumber] = $"<< /Length {Encoding.ASCII.GetByteCount(content)} >>\nstream\n{content}\nendstream";
            }

            objects[2] = $"<< /Type /Pages /Kids [{kids}] /Count {pages.Count} >>";
            objects[normalFontObjectNumber] = "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>";
            objects[boldFontObjectNumber] = "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>";

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true);

            writer.WriteLine("%PDF-1.4");
            var offsets = new long[objectCount + 1];

            for (var i = 1; i <= objectCount; i++)
            {
                offsets[i] = stream.Position;
                writer.WriteLine($"{i} 0 obj");
                writer.WriteLine(objects[i]);
                writer.WriteLine("endobj");
                writer.Flush();
            }

            var xrefPosition = stream.Position;
            writer.WriteLine("xref");
            writer.WriteLine($"0 {objectCount + 1}");
            writer.WriteLine("0000000000 65535 f ");

            for (var i = 1; i <= objectCount; i++)
                writer.WriteLine($"{offsets[i]:0000000000} 00000 n ");

            writer.WriteLine("trailer");
            writer.WriteLine($"<< /Size {objectCount + 1} /Root 1 0 R >>");
            writer.WriteLine("startxref");
            writer.WriteLine(xrefPosition);
            writer.WriteLine("%%EOF");
            writer.Flush();

            return stream.ToArray();
        }

        private static List<string> BuildPages(
            string title,
            IReadOnlyList<PdfColumn> columns,
            IReadOnlyList<IReadOnlyList<string>> rows,
            string summary)
        {
            const float pageWidth = 595;
            const float tableX = 42;
            const float topY = 760;
            const float bottomY = 94;
            const float headerHeight = 18;

            var pages = new List<string>();
            var content = StartPage(title, pageWidth);
            var y = topY;

            DrawHeader(content, columns, tableX, y, headerHeight);
            y -= headerHeight;

            if (rows.Count == 0)
            {
                DrawEmptyRow(content, tableX, y, columns.Sum(c => c.Width));
                y -= 28;
            }
            else
            {
                foreach (var row in rows)
                {
                    var wrapped = WrapRow(row, columns);
                    var rowHeight = Math.Max(30, wrapped.Max(cell => cell.Count) * 11 + 12);

                    if (y - rowHeight < bottomY)
                    {
                        pages.Add(FinishPage(content, pages.Count + 1));
                        content = StartPage(title, pageWidth);
                        y = topY;
                        DrawHeader(content, columns, tableX, y, headerHeight);
                        y -= headerHeight;
                    }

                    DrawDataRow(content, columns, wrapped, tableX, y, rowHeight);
                    y -= rowHeight;
                }
            }

            y -= 32;
            if (y < bottomY)
            {
                pages.Add(FinishPage(content, pages.Count + 1));
                content = StartPage(title, pageWidth);
                y = topY - headerHeight - 32;
            }

            Text(content, "F2", 11, tableX, y, summary);
            pages.Add(FinishPage(content, pages.Count + 1));

            var totalPages = pages.Count;
            for (var i = 0; i < totalPages; i++)
                pages[i] = pages[i].Replace("__TOTAL_PAGES__", totalPages.ToString());

            return pages;
        }

        private static StringBuilder StartPage(string title, float pageWidth)
        {
            var content = new StringBuilder();
            content.AppendLine("0 0 0 rg 0 0 0 RG 0.8 w");

            var titleX = Math.Max(42, (pageWidth - title.Length * 8.4f) / 2);
            Text(content, "F2", 16, titleX, 790, title);
            Text(content, "F1", 10, 424, 766, $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}");

            return content;
        }

        private static string FinishPage(StringBuilder content, int page)
        {
            Text(content, "F1", 8, 42, 36, $"Pagina {page} de __TOTAL_PAGES__");
            return content.ToString();
        }

        private static void DrawHeader(StringBuilder content, IReadOnlyList<PdfColumn> columns, float tableX, float topY, float height)
        {
            var x = tableX;
            foreach (var column in columns)
            {
                FillRect(content, x, topY - height, column.Width, height, 0.78f, 0, 0);
                StrokeRect(content, x, topY - height, column.Width, height);
                Text(content, "F2", 9, x + 3, topY - 13, column.Header, "1 1 1");
                x += column.Width;
            }

            content.AppendLine("0 0 0 rg");
        }

        private static void DrawDataRow(
            StringBuilder content,
            IReadOnlyList<PdfColumn> columns,
            IReadOnlyList<IReadOnlyList<string>> wrapped,
            float tableX,
            float topY,
            float height)
        {
            var x = tableX;
            for (var i = 0; i < columns.Count; i++)
            {
                StrokeRect(content, x, topY - height, columns[i].Width, height);

                for (var line = 0; line < wrapped[i].Count; line++)
                    Text(content, "F1", 9, x + 5, topY - 13 - line * 10, wrapped[i][line]);

                x += columns[i].Width;
            }
        }

        private static void DrawEmptyRow(StringBuilder content, float tableX, float topY, float width)
        {
            const float height = 28;
            StrokeRect(content, tableX, topY - height, width, height);
            Text(content, "F1", 9, tableX + 6, topY - 17, "No hay registros para este reporte.");
        }

        private static void FillRect(StringBuilder content, float x, float y, float width, float height, float r, float g, float b)
        {
            content.AppendLine($"{Number(r)} {Number(g)} {Number(b)} rg");
            content.AppendLine($"{Number(x)} {Number(y)} {Number(width)} {Number(height)} re f");
            content.AppendLine("0 0 0 rg");
        }

        private static void StrokeRect(StringBuilder content, float x, float y, float width, float height)
        {
            content.AppendLine($"0 0 0 RG {Number(x)} {Number(y)} {Number(width)} {Number(height)} re S");
        }

        private static void Text(StringBuilder content, string font, int size, float x, float y, string value, string color = "0 0 0")
        {
            content.AppendLine($"{color} rg BT /{font} {size} Tf {Number(x)} {Number(y)} Td ({EscapePdf(value)}) Tj ET");
        }

        private static IReadOnlyList<IReadOnlyList<string>> WrapRow(IReadOnlyList<string> row, IReadOnlyList<PdfColumn> columns)
        {
            var cells = new List<IReadOnlyList<string>>();
            for (var i = 0; i < columns.Count; i++)
            {
                var value = i < row.Count ? row[i] : string.Empty;
                var maxCharacters = Math.Max(6, (int)(columns[i].Width / 5.2f));
                cells.Add(Wrap(value, maxCharacters));
            }

            return cells;
        }

        private static IReadOnlyList<string> Wrap(string value, int maxCharacters)
        {
            value = string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
            var words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var lines = new List<string>();
            var current = string.Empty;

            foreach (var word in words)
            {
                if (word.Length > maxCharacters)
                {
                    if (!string.IsNullOrWhiteSpace(current))
                    {
                        lines.Add(current);
                        current = string.Empty;
                    }

                    for (var i = 0; i < word.Length; i += maxCharacters)
                        lines.Add(word.Substring(i, Math.Min(maxCharacters, word.Length - i)));

                    continue;
                }

                var candidate = string.IsNullOrWhiteSpace(current) ? word : $"{current} {word}";
                if (candidate.Length > maxCharacters)
                {
                    lines.Add(current);
                    current = word;
                }
                else
                {
                    current = candidate;
                }
            }

            if (!string.IsNullOrWhiteSpace(current))
                lines.Add(current);

            return lines.Count == 0 ? new[] { "-" } : lines;
        }

        private static string FullName(Cliente cliente)
        {
            return $"{cliente.Nombres} {cliente.Apellidos}".Trim();
        }

        private static string Date(DateTime value)
        {
            return value.ToString("dd/MM/yyyy", ReportCulture);
        }

        private static string Money(decimal? value)
        {
            return Money(value ?? 0);
        }

        private static string Money(decimal value)
        {
            return $"S/ {value.ToString("N2", ReportCulture)}";
        }

        private static string Number(float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static string EscapePdf(string value)
        {
            var normalized = RemoveDiacritics(value)
                .Replace("\\", "\\\\")
                .Replace("(", "\\(")
                .Replace(")", "\\)");

            var builder = new StringBuilder(normalized.Length);
            foreach (var character in normalized)
                builder.Append(character <= 127 ? character : '?');

            return builder.ToString();
        }

        private static string RemoveDiacritics(string text)
        {
            var normalized = text.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);

            foreach (var character in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(character);
                if (category != UnicodeCategory.NonSpacingMark)
                    builder.Append(character);
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }

        private sealed record PdfColumn(string Header, float Width);
    }
}
