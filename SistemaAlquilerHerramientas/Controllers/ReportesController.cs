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
                new PdfColumn("ID", 42, "center"),
                new PdfColumn("Herramienta", 140),
                new PdfColumn("Categoria", 106),
                new PdfColumn("Proveedor", 122),
                new PdfColumn("Precio Dia", 72, "right"),
                new PdfColumn("Estado", 65, "center")
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
                new PdfColumn("ID Alq", 50, "center"),
                new PdfColumn("Cliente", 128),
                new PdfColumn("Herramienta", 120),
                new PdfColumn("Entrega", 68, "center"),
                new PdfColumn("Devolucion", 76, "center"),
                new PdfColumn("Monto", 62, "right"),
                new PdfColumn("Estado", 55, "center")
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
                new PdfColumn("ID Reserva", 70, "center"),
                new PdfColumn("Cliente", 142),
                new PdfColumn("Herramienta", 138),
                new PdfColumn("Inicio", 72, "center"),
                new PdfColumn("Devolucion", 82, "center"),
                new PdfColumn("Estado", 55, "center")
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
                new PdfColumn("ID Dev", 50, "center"),
                new PdfColumn("ID Alq", 50, "center"),
                new PdfColumn("Cliente", 130),
                new PdfColumn("Herramienta", 132),
                new PdfColumn("Fecha", 76, "center"),
                new PdfColumn("Estado", 88, "center")
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
                new PdfColumn("ID Mora", 57, "center"),
                new PdfColumn("ID Alq", 56, "center"),
                new PdfColumn("Cliente", 113),
                new PdfColumn("Herramienta", 113),
                new PdfColumn("Dias Retraso", 74, "right"),
                new PdfColumn("Monto Mora", 74, "right"),
                new PdfColumn("Estado Pago", 73, "center")
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
                objects[pageObjectNumber] = $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 842 595] /Resources << /Font << /F1 {normalFontObjectNumber} 0 R /F2 {boldFontObjectNumber} 0 R >> >> /Contents {contentObjectNumber} 0 R >>";
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
            const float pageWidth = 842;
            const float pageHeight = 595;
            const float tableX = 38;
            const float tableWidth = pageWidth - tableX * 2;
            const float topY = 466;
            const float bottomY = 74;
            const float headerHeight = 24;

            var pages = new List<string>();
            var reportColumns = NormalizeColumns(columns, tableWidth);
            var content = StartPage(title, pageWidth, pageHeight, tableX, tableWidth);
            var y = topY;

            DrawHeader(content, reportColumns, tableX, y, headerHeight);
            y -= headerHeight;

            if (rows.Count == 0)
            {
                DrawEmptyRow(content, tableX, y, tableWidth);
                y -= 28;
            }
            else
            {
                var rowIndex = 0;
                foreach (var row in rows)
                {
                    var wrapped = WrapRow(row, reportColumns);
                    var rowHeight = Math.Max(34, wrapped.Max(cell => cell.Count) * 11 + 16);

                    if (y - rowHeight < bottomY)
                    {
                        pages.Add(FinishPage(content, pages.Count + 1));
                        content = StartPage(title, pageWidth, pageHeight, tableX, tableWidth);
                        y = topY;
                        DrawHeader(content, reportColumns, tableX, y, headerHeight);
                        y -= headerHeight;
                        rowIndex = 0;
                    }

                    DrawDataRow(content, reportColumns, wrapped, tableX, y, rowHeight, rowIndex);
                    y -= rowHeight;
                    rowIndex++;
                }
            }

            y -= 26;
            if (y < bottomY)
            {
                pages.Add(FinishPage(content, pages.Count + 1));
                content = StartPage(title, pageWidth, pageHeight, tableX, tableWidth);
                y = topY - headerHeight - 26;
            }

            FillRect(content, tableX, y - 18, tableWidth, 30, 0.98f, 0.94f, 0.93f);
            StrokeRect(content, tableX, y - 18, tableWidth, 30, 0.82f, 0.24f, 0.18f);
            Text(content, "F2", 10, tableX + 12, y - 6, summary);
            pages.Add(FinishPage(content, pages.Count + 1));

            var totalPages = pages.Count;
            for (var i = 0; i < totalPages; i++)
                pages[i] = pages[i].Replace("__TOTAL_PAGES__", totalPages.ToString());

            return pages;
        }

        private static StringBuilder StartPage(string title, float pageWidth, float pageHeight, float tableX, float tableWidth)
        {
            var content = new StringBuilder();
            content.AppendLine("0 0 0 rg 0 0 0 RG 0.6 w");

            FillRect(content, 0, pageHeight - 48, pageWidth, 48, 0.13f, 0.16f, 0.20f);
            FillRect(content, tableX, pageHeight - 58, 112, 6, 0.86f, 0.08f, 0.06f);
            Text(content, "F2", 18, tableX, pageHeight - 32, title, "1 1 1");
            Text(content, "F1", 9, tableX, pageHeight - 74, "Sistema de alquiler de herramientas");
            TextAligned(content, "F1", 9, tableX, pageHeight - 74, $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}", "right", "0 0 0", tableWidth);

            return content;
        }

        private static string FinishPage(StringBuilder content, int page)
        {
            StrokeLine(content, 38, 48, 804, 48, 0.82f, 0.84f, 0.86f);
            Text(content, "F1", 8, 38, 32, "Reporte generado automaticamente");
            TextAligned(content, "F1", 8, 38, 32, $"Pagina {page} de __TOTAL_PAGES__", "center", "0 0 0", 766);
            return content.ToString();
        }

        private static void DrawHeader(StringBuilder content, IReadOnlyList<PdfColumn> columns, float tableX, float topY, float height)
        {
            var x = tableX;
            foreach (var column in columns)
            {
                FillRect(content, x, topY - height, column.Width, height, 0.86f, 0.08f, 0.06f);
                StrokeRect(content, x, topY - height, column.Width, height, 0.68f, 0.06f, 0.05f);
                TextAligned(content, "F2", 8, x + 7, topY - 15, column.Header, column.Alignment, "1 1 1", column.Width - 14);
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
            float height,
            int rowIndex)
        {
            var totalWidth = columns.Sum(c => c.Width);
            if (rowIndex % 2 == 1)
            {
                FillRect(content, tableX, topY - height, totalWidth, height, 0.98f, 0.98f, 0.98f);
            }

            var x = tableX;
            for (var i = 0; i < columns.Count; i++)
            {
                StrokeRect(content, x, topY - height, columns[i].Width, height, 0.78f, 0.80f, 0.83f);

                for (var line = 0; line < wrapped[i].Count; line++)
                    TextAligned(content, "F1", 8, x + 7, topY - 15 - line * 10, wrapped[i][line], columns[i].Alignment, "0.10 0.13 0.18", columns[i].Width - 14);

                x += columns[i].Width;
            }
        }

        private static void DrawEmptyRow(StringBuilder content, float tableX, float topY, float width)
        {
            const float height = 28;
            FillRect(content, tableX, topY - height, width, height, 0.98f, 0.98f, 0.98f);
            StrokeRect(content, tableX, topY - height, width, height, 0.78f, 0.80f, 0.83f);
            Text(content, "F1", 9, tableX + 10, topY - 17, "No hay registros para este reporte.");
        }

        private static void FillRect(StringBuilder content, float x, float y, float width, float height, float r, float g, float b)
        {
            content.AppendLine($"{Number(r)} {Number(g)} {Number(b)} rg");
            content.AppendLine($"{Number(x)} {Number(y)} {Number(width)} {Number(height)} re f");
            content.AppendLine("0 0 0 rg");
        }

        private static void StrokeRect(StringBuilder content, float x, float y, float width, float height, float r = 0, float g = 0, float b = 0)
        {
            content.AppendLine($"{Number(r)} {Number(g)} {Number(b)} RG {Number(x)} {Number(y)} {Number(width)} {Number(height)} re S");
            content.AppendLine("0 0 0 RG");
        }

        private static void StrokeLine(StringBuilder content, float x1, float y1, float x2, float y2, float r, float g, float b)
        {
            content.AppendLine($"{Number(r)} {Number(g)} {Number(b)} RG {Number(x1)} {Number(y1)} m {Number(x2)} {Number(y2)} l S");
            content.AppendLine("0 0 0 RG");
        }

        private static void Text(StringBuilder content, string font, int size, float x, float y, string value, string color = "0 0 0")
        {
            content.AppendLine($"{color} rg BT /{font} {size} Tf {Number(x)} {Number(y)} Td ({EscapePdf(value)}) Tj ET");
        }

        private static void TextAligned(
            StringBuilder content,
            string font,
            int size,
            float x,
            float y,
            string value,
            string alignment,
            string color = "0 0 0",
            float width = 0)
        {
            var textWidth = EstimateTextWidth(value, size);
            var textX = alignment switch
            {
                "right" => x + Math.Max(0, width - textWidth),
                "center" => x + Math.Max(0, (width - textWidth) / 2),
                _ => x
            };

            Text(content, font, size, textX, y, value, color);
        }

        private static IReadOnlyList<PdfColumn> NormalizeColumns(IReadOnlyList<PdfColumn> columns, float tableWidth)
        {
            var originalWidth = columns.Sum(c => c.Width);
            if (originalWidth <= 0)
            {
                return columns;
            }

            var scale = tableWidth / originalWidth;
            return columns
                .Select(c => c with { Width = c.Width * scale })
                .ToList();
        }

        private static IReadOnlyList<IReadOnlyList<string>> WrapRow(IReadOnlyList<string> row, IReadOnlyList<PdfColumn> columns)
        {
            var cells = new List<IReadOnlyList<string>>();
            for (var i = 0; i < columns.Count; i++)
            {
                var value = i < row.Count ? row[i] : string.Empty;
                var maxCharacters = Math.Max(7, (int)(columns[i].Width / 4.4f));
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

        private static float EstimateTextWidth(string value, int size)
        {
            return (value?.Length ?? 0) * size * 0.48f;
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

        private sealed record PdfColumn(string Header, float Width, string Alignment = "left");
    }
}
