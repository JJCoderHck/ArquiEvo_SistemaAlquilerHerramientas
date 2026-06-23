using System;
using System.Collections.Generic;

namespace SistemaAlquilerHerramientas.Models;

public partial class Cliente
{
    public int IdCliente { get; set; }

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string Dni { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Correo { get; set; }

    public string? Direccion { get; set; }

    public string? Estado { get; set; }

    public virtual ICollection<Alquiler> Alquilers { get; set; } = new List<Alquiler>();

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
