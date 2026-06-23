using System;
using System.Collections.Generic;

namespace SistemaAlquilerHerramientas.Models;

public partial class Herramientum
{
    public int IdHerramienta { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal PrecioPorDia { get; set; }

    public string? EstadoHerramienta { get; set; }

    public int IdCategoria { get; set; }

    public int IdProveedor { get; set; }

    public virtual ICollection<Alquiler> Alquilers { get; set; } = new List<Alquiler>();

    public virtual CategoriaHerramientum? IdCategoriaNavigation { get; set; }
    public virtual Proveedor? IdProveedorNavigation { get; set; }

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
