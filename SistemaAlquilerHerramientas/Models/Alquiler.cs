using System;
using System.Collections.Generic;

namespace SistemaAlquilerHerramientas.Models;

public partial class Alquiler
{
    public int IdAlquiler { get; set; }

    public int IdCliente { get; set; }

    public int IdHerramienta { get; set; }

    public int? IdReserva { get; set; }

    public DateTime FechaEntrega { get; set; }

    public DateTime FechaDevolucionPactada { get; set; }

    public decimal? MontoEstimado { get; set; }

    public string? EstadoAlquiler { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public virtual ICollection<Devolucion> Devolucions { get; set; } = new List<Devolucion>();

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Herramientum IdHerramientaNavigation { get; set; } = null!;

    public virtual Reserva? IdReservaNavigation { get; set; }

    public virtual ICollection<Mora> Moras { get; set; } = new List<Mora>();
}
