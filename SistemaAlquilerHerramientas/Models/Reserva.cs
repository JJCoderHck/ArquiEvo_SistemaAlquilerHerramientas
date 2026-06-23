using System;
using System.Collections.Generic;

namespace SistemaAlquilerHerramientas.Models;

public partial class Reserva
{
    public int IdReserva { get; set; }

    public int IdCliente { get; set; }

    public int IdHerramienta { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime FechaDevolucionEstimada { get; set; }

    public string? EstadoReserva { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public virtual ICollection<Alquiler> Alquilers { get; set; } = new List<Alquiler>();
    public virtual Cliente? IdClienteNavigation { get; set; }
    public virtual Herramientum? IdHerramientaNavigation { get; set; }
}
