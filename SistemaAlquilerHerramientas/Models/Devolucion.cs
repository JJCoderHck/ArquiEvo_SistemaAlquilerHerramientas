using System;
using System.Collections.Generic;

namespace SistemaAlquilerHerramientas.Models;

public partial class Devolucion
{
    public int IdDevolucion { get; set; }

    public int IdAlquiler { get; set; }

    public DateTime FechaDevolucionReal { get; set; }

    public string? EstadoRetorno { get; set; }

    public string? Observacion { get; set; }

    public DateTime? FechaRegistro { get; set; }

    public virtual Alquiler IdAlquilerNavigation { get; set; } = null!;
}
