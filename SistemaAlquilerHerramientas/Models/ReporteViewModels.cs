namespace SistemaAlquilerHerramientas.Models
{
    public class ReporteHerramientaViewModel
    {
        public int IdHerramienta { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal PrecioPorDia { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string Proveedor { get; set; } = string.Empty;
        public string? Estado { get; set; }
    }

    public class ReporteAlquilerViewModel
    {
        public int IdAlquiler { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string Herramienta { get; set; } = string.Empty;
        public DateTime FechaEntrega { get; set; }
        public DateTime FechaDevolucionPactada { get; set; }
        public decimal MontoEstimado { get; set; }
        public string? Estado { get; set; }
        public int DiasTranscurridos { get; set; }
    }

    public class ReporteReservaViewModel
    {
        public int IdReserva { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string Herramienta { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaDevolucionEstimada { get; set; }
        public string? Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int DiasReserva { get; set; }
    }

    public class ReporteDevolucionViewModel
    {
        public int IdDevolucion { get; set; }
        public int IdAlquiler { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string Herramienta { get; set; } = string.Empty;
        public DateTime FechaDevolucionReal { get; set; }
        public DateTime FechaDevolucionPactada { get; set; }
        public string? EstadoRetorno { get; set; }
        public string? Observacion { get; set; }
        public int DiasRetraso { get; set; }
    }

    public class ReporteMoraViewModel
    {
        public int IdMora { get; set; }
        public int IdAlquiler { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string Herramienta { get; set; } = string.Empty;
        public int DiasRetraso { get; set; }
        public decimal MontoMora { get; set; }
        public string? EstadoPago { get; set; }
    }
}
