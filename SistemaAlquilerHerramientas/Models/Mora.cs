using System;
using System.Collections.Generic;

namespace SistemaAlquilerHerramientas.Models;

public partial class Mora
{
    public int IdMora { get; set; }

    public int IdAlquiler { get; set; }

    public int DiasRetraso { get; set; }

    public decimal MontoMora { get; set; }

    public string? EstadoPago { get; set; }

    public virtual Alquiler? IdAlquilerNavigation { get; set; }
}
