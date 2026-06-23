using System;
using System.Collections.Generic;

namespace SistemaAlquilerHerramientas.Models;

public partial class CategoriaHerramientum
{
    public int IdCategoria { get; set; }

    public string NombreCategoria { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? Estado { get; set; }

    public virtual ICollection<Herramientum> Herramienta { get; set; } = new List<Herramientum>();
}
