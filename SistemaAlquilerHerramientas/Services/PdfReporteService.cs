using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections.Generic;
using System.IO;
using SistemaAlquilerHerramientas.Models;

namespace SistemaAlquilerHerramientas.Services
{
    public class PdfReporteService
    {
        public byte[] GenerarPdfHerramientasDisponibles(List<ReporteHerramientaViewModel> herramientas)
        {
            using (var ms = new MemoryStream())
            {
                var documento = new Document();
                PdfWriter.GetInstance(documento, ms);
                documento.Open();

                // Título
                var titulo = new Paragraph("REPORTE DE HERRAMIENTAS DISPONIBLES",
                    FontFactory.GetFont("Arial", 16, Font.BOLD));
                titulo.Alignment = Element.ALIGN_CENTER;
                documento.Add(titulo);

                // Fecha
                var fecha = new Paragraph($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}",
                    FontFactory.GetFont("Arial", 10));
                fecha.Alignment = Element.ALIGN_RIGHT;
                documento.Add(fecha);
                documento.Add(new Paragraph(" "));

                // Tabla
                var tabla = new PdfPTable(7) { WidthPercentage = 100 };
                tabla.SetWidths(new float[] { 0.8f, 1.5f, 2f, 1f, 1.5f, 1.5f, 1f });

                // Encabezados
                var headerFont = FontFactory.GetFont("Arial", 10, Font.BOLD, BaseColor.WHITE);
                var headerCell = new PdfPCell { BackgroundColor = new BaseColor(0, 102, 204) };

                foreach (var header in new[] { "ID", "Nombre", "Descripción", "Precio/Día", "Categoría", "Proveedor", "Estado" })
                {
                    headerCell.Phrase = new Phrase(header, headerFont);
                    tabla.AddCell(headerCell);
                }

                // Datos
                var normalFont = FontFactory.GetFont("Arial", 9);
                foreach (var herramienta in herramientas)
                {
                    tabla.AddCell(new PdfPCell(new Phrase(herramienta.IdHerramienta.ToString(), normalFont)) { Padding = 5 });
                    tabla.AddCell(new PdfPCell(new Phrase(herramienta.Nombre, normalFont)) { Padding = 5 });
                    tabla.AddCell(new PdfPCell(new Phrase(herramienta.Descripcion ?? "-", normalFont)) { Padding = 5 });
                    tabla.AddCell(new PdfPCell(new Phrase(herramienta.PrecioPorDia.ToString("C"), normalFont)) { Padding = 5 });
                    tabla.AddCell(new PdfPCell(new Phrase(herramienta.Categoria, normalFont)) { Padding = 5 });
                    tabla.AddCell(new PdfPCell(new Phrase(herramienta.Proveedor, normalFont)) { Padding = 5 });
                    tabla.AddCell(new PdfPCell(new Phrase(herramienta.Estado ?? "-", normalFont)) { Padding = 5 });
                }

                documento.Add(tabla);

                // Resumen
                documento.Add(new Paragraph(" "));
                var resumen = new Paragraph($"Total de herramientas: {herramientas.Count}",
                    FontFactory.GetFont("Arial", 11, Font.BOLD));
                documento.Add(resumen);

                documento.Close();
                return ms.ToArray();
            }
        }

        public byte[] GenerarPdfAlquileresActivos(List<ReporteAlquilerViewModel> alquileres)
        {
            using (var ms = new MemoryStream())
            {
                var documento = new Document();
                PdfWriter.GetInstance(documento, ms);
                documento.Open();

                var titulo = new Paragraph("REPORTE DE ALQUILERES ACTIVOS",
                    FontFactory.GetFont("Arial", 16, Font.BOLD));
                titulo.Alignment = Element.ALIGN_CENTER;
                documento.Add(titulo);

                var fecha = new Paragraph($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}",
                    FontFactory.GetFont("Arial", 10));
                fecha.Alignment = Element.ALIGN_RIGHT;
                documento.Add(fecha);
                documento.Add(new Paragraph(" "));

                var tabla = new PdfPTable(8) { WidthPercentage = 100 };
                tabla.SetWidths(new float[] { 0.8f, 1.5f, 1.5f, 1.2f, 1.2f, 1f, 1f, 0.8f });

                var headerFont = FontFactory.GetFont("Arial", 10, Font.BOLD, BaseColor.WHITE);
                var headerCell = new PdfPCell { BackgroundColor = new BaseColor(51, 153, 0) };

                foreach (var header in new[] { "ID", "Cliente", "Herramienta", "F. Entrega", "F. Devolución", "Días", "Monto", "Estado" })
                {
                    headerCell.Phrase = new Phrase(header, headerFont);
                    tabla.AddCell(headerCell);
                }

                var normalFont = FontFactory.GetFont("Arial", 9);
                foreach (var alquiler in alquileres)
                {
                    tabla.AddCell(new PdfPCell(new Phrase(alquiler.IdAlquiler.ToString(), normalFont)) { Padding = 5 });
                    tabla.AddCell(new PdfPCell(new Phrase(alquiler.Cliente, normalFont)) { Padding = 5 });
                    tabla.AddCell(new PdfPCell(new Phrase(alquiler.Herramienta, normalFont)) { Padding = 5 });
                    tabla.AddCell(new PdfPCell(new Phrase(alquiler.FechaEntrega.ToString("dd/MM/yyyy"), normalFont)) { Padding = 5 });
                    tabla.AddCell(new PdfPCell(new Phrase(alquiler.FechaDevolucionPactada.ToString("dd/MM/yyyy"), normalFont)) { Padding = 5 });
                    tabla.AddCell(new PdfPCell(new Phrase(alquiler.DiasTranscurridos.ToString(), normalFont)) { Padding = 5 });
                    tabla.AddCell(new PdfPCell(new Phrase(alquiler.MontoEstimado.ToString("C"), normalFont)) { Padding = 5 });
                    tabla.AddCell(new PdfPCell(new Phrase(alquiler.Estado ?? "-", normalFont)) { Padding = 5 });
                }

                documento.Add(tabla);

                documento.Add(new Paragraph(" "));
                var resumen = new Paragraph($"Total de alquileres: {alquileres.Count} | Monto total: {alquileres.Sum(a => a.MontoEstimado):C}",
                    FontFactory.GetFont("Arial", 11, Font.BOLD));
                documento.Add(resumen);

                documento.Close();
                return ms.ToArray();
            }
        }

        public byte[] GenerarPdfReservas(List<ReporteReservaViewModel> reservas)
        {
            using (var ms = new MemoryStream())
            {
                var documento = new Document();
                PdfWriter.GetInstance(documento, ms);
                documento.Open();

                var titulo = new Paragraph("REPORTE DE RESERVAS",
                    FontFactory.GetFont("Arial", 16, Font.BOLD));
                titulo.Alignment = Element.ALIGN_CENTER;
                documento.Add(titulo);

                var fecha = new Paragraph($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}",
                    FontFactory.GetFont("Arial", 10));
                fecha.Alignment = Element.ALIGN_RIGHT;
                documento.Add(fecha);
                documento.Add(new Paragraph(" "));

                var tabla = new PdfPTable(8) { WidthPercentage = 100 };
                tabla.SetWidths(new float[] { 0.8f, 1.5f, 1.5f, 1.2f, 1.2f, 1f, 1.2f, 0.8f });

                var headerFont = FontFactory.GetFont("Arial", 10, Font.BOLD, BaseColor.WHITE);
                var headerCell = new PdfPCell { BackgroundColor = new BaseColor(102, 102, 255) };

                foreach (var header in new[] { "ID", "Cliente", "Herramienta", "F. Inicio", "F. Devolución", "Días", "F. Registro", "Estado" })
                {
                    headerCell.Phrase = new Phrase(header, headerFont);
                    tabla.AddCell(headerCell);
                }

                var normalFont = FontFactory.GetFont("Arial", 9);
                foreach (var reserva in reservas)
                {
                    tabla.AddCell(new PdfPCell(new Phrase(reserva.IdReserva.ToString(), normalFont)) { Padding = 5 });
                    tabla.AddCell(new PdfPCell(new Phrase(reserva.Cliente, normalFont)) { Padding = 5 });
                    tabla.AddCell(new PdfPCell(new Phrase(reserva.Herramienta, normalFont)) { Padding = 5 });
                    tabla.AddCell(new PdfPCell(new Phrase(reserva.FechaInicio.ToString("dd/MM/yyyy"), normalFont)) { Padding = 5 });
                    tabla.AddCell(new PdfPCell(new Phrase(reserva.FechaDevolucionEstimada.ToString("dd/MM/yyyy"), normalFont)) { Padding = 5 });
                    tabla.AddCell(new PdfPCell(new Phrase(reserva.DiasReserva.ToString(), normalFont)) { Padding = 5 });
                    tabla.AddCell(new PdfPCell(new Phrase(reserva.FechaRegistro.ToString("dd/MM/yyyy HH:mm"), normalFont)) { Padding = 5 });
                    tabla.AddCell(new PdfPCell(new Phrase(reserva.Estado ?? "-", normalFont)) { Padding = 5 });
                }

                documento.Add(tabla);

                documento.Add(new Paragraph(" "));
                var resumen = new Paragraph($"Total de reservas: {reservas.Count}",
                    FontFactory.GetFont("Arial", 11, Font.BOLD));
                documento.Add(resumen);

                documento.Close();
                return ms.ToArray();
            }
        }

        public byte[] GenerarPdfDevoluciones(List<ReporteDevolucionViewModel> devoluciones)
        {
            using (var ms = new MemoryStream())
            {
                var documento = new Document();
                PdfWriter.GetInstance(documento, ms);
                documento.Open();

                var titulo = new Paragraph("REPORTE DE DEVOLUCIONES",
                    FontFactory.GetFont("Arial", 16, Font.BOLD));
                titulo.Alignment = Element.ALIGN_CENTER;
                documento.Add(titulo);

                var fecha = new Paragraph($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}",
                    FontFactory.GetFont("Arial", 10));
                fecha.Alignment = Element.ALIGN_RIGHT;
                documento.Add(fecha);
                documento.Add(new Paragraph(" "));

                var tabla = new PdfPTable(9) { WidthPercentage = 100 };
                tabla.SetWidths(new float[] { 0.7f, 0.7f, 1.3f, 1.3f, 1f, 1f, 0.8f, 0.8f, 1.5f });

                var headerFont = FontFactory.GetFont("Arial", 9, Font.BOLD, BaseColor.WHITE);
                var headerCell = new PdfPCell { BackgroundColor = new BaseColor(255, 153, 0) };

                foreach (var header in new[] { "ID Dev", "ID Alq", "Cliente", "Herramienta", "F. Pactada", "F. Real", "Retraso", "Estado", "Observación" })
                {
                    headerCell.Phrase = new Phrase(header, headerFont);
                    tabla.AddCell(headerCell);
                }

                var normalFont = FontFactory.GetFont("Arial", 8);
                foreach (var devolucion in devoluciones)
                {
                    tabla.AddCell(new PdfPCell(new Phrase(devolucion.IdDevolucion.ToString(), normalFont)) { Padding = 4 });
                    tabla.AddCell(new PdfPCell(new Phrase(devolucion.IdAlquiler.ToString(), normalFont)) { Padding = 4 });
                    tabla.AddCell(new PdfPCell(new Phrase(devolucion.Cliente, normalFont)) { Padding = 4 });
                    tabla.AddCell(new PdfPCell(new Phrase(devolucion.Herramienta, normalFont)) { Padding = 4 });
                    tabla.AddCell(new PdfPCell(new Phrase(devolucion.FechaDevolucionPactada.ToString("dd/MM/yyyy"), normalFont)) { Padding = 4 });
                    tabla.AddCell(new PdfPCell(new Phrase(devolucion.FechaDevolucionReal.ToString("dd/MM/yyyy"), normalFont)) { Padding = 4 });
                    tabla.AddCell(new PdfPCell(new Phrase(devolucion.DiasRetraso.ToString(), normalFont)) { Padding = 4 });
                    tabla.AddCell(new PdfPCell(new Phrase(devolucion.EstadoRetorno ?? "-", normalFont)) { Padding = 4 });
                    tabla.AddCell(new PdfPCell(new Phrase(devolucion.Observacion ?? "-", normalFont)) { Padding = 4 });
                }

                documento.Add(tabla);

                documento.Add(new Paragraph(" "));
                var retrasadas = devoluciones.Count(d => d.DiasRetraso > 0);
                var resumen = new Paragraph($"Total de devoluciones: {devoluciones.Count} | Retrasadas: {retrasadas}",
                    FontFactory.GetFont("Arial", 11, Font.BOLD));
                documento.Add(resumen);

                documento.Close();
                return ms.ToArray();
            }
        }

        public byte[] GenerarPdfMoras(List<ReporteMoraViewModel> moras)
        {
            using (var ms = new MemoryStream())
            {
                var documento = new Document();
                PdfWriter.GetInstance(documento, ms);
                documento.Open();

                var titulo = new Paragraph("REPORTE DE MORAS",
                    FontFactory.GetFont("Arial", 16, Font.BOLD));
                titulo.Alignment = Element.ALIGN_CENTER;
                documento.Add(titulo);

                var fecha = new Paragraph($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}",
                    FontFactory.GetFont("Arial", 10));
                fecha.Alignment = Element.ALIGN_RIGHT;
                documento.Add(fecha);
                documento.Add(new Paragraph(" "));

                var tabla = new PdfPTable(7) { WidthPercentage = 100 };
                tabla.SetWidths(new float[] { 0.8f, 0.8f, 1.5f, 1.5f, 1f, 1f, 1f });

                var headerFont = FontFactory.GetFont("Arial", 10, Font.BOLD, BaseColor.WHITE);
                var headerCell = new PdfPCell { BackgroundColor = new BaseColor(204, 0, 0) };

                foreach (var header in new[] { "ID Mora", "ID Alq", "Cliente", "Herramienta", "Días Retraso", "Monto Mora", "Estado Pago" })
                {
                    headerCell.Phrase = new Phrase(header, headerFont);
                    tabla.AddCell(headerCell);
                }

                var normalFont = FontFactory.GetFont("Arial", 9);
                foreach (var mora in moras)
                {
                    tabla.AddCell(new PdfPCell(new Phrase(mora.IdMora.ToString(), normalFont)) { Padding = 5 });
                    tabla.AddCell(new PdfPCell(new Phrase(mora.IdAlquiler.ToString(), normalFont)) { Padding = 5 });
                    tabla.AddCell(new PdfPCell(new Phrase(mora.Cliente, normalFont)) { Padding = 5 });
                    tabla.AddCell(new PdfPCell(new Phrase(mora.Herramienta, normalFont)) { Padding = 5 });
                    tabla.AddCell(new PdfPCell(new Phrase(mora.DiasRetraso.ToString(), normalFont)) { Padding = 5 });
                    tabla.AddCell(new PdfPCell(new Phrase(mora.MontoMora.ToString("C"), normalFont)) { Padding = 5 });
                    tabla.AddCell(new PdfPCell(new Phrase(mora.EstadoPago ?? "-", normalFont)) { Padding = 5 });
                }

                documento.Add(tabla);

                documento.Add(new Paragraph(" "));
                var pendientes = moras.Count(m => m.EstadoPago == "Pendiente");
                var resumen = new Paragraph($"Total de moras: {moras.Count} | Pendientes: {pendientes} | Monto total: {moras.Sum(m => m.MontoMora):C}",
                    FontFactory.GetFont("Arial", 11, Font.BOLD));
                documento.Add(resumen);

                documento.Close();
                return ms.ToArray();
            }
        }
    }
}
