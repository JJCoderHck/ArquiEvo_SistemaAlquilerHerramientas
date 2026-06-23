using SistemaAlquilerHerramientas.Models;

namespace SistemaAlquilerHerramientas.Services
{
    public class HerramientaEstadoService
    {
        private readonly AlquilerHerramientasContext _context;

        public HerramientaEstadoService(AlquilerHerramientasContext context)
        {
            _context = context;
        }

        // Estados válidos como constantes para evitar typos
        public static class Estados
        {
            public const string Disponible = "Disponible";
            public const string Reservada = "Reservada";
            public const string Alquilada = "Alquilada";
            public const string Mantenimiento = "Mantenimiento";
            public const string DadaDeBaja = "Dada de baja";
        }

        // Cambia el estado de la herramienta validando que la transición sea válida
        public (bool exito, string mensaje) CambiarEstado(int idHerramienta, string nuevoEstado)
        {
            var herramienta = _context.Herramienta.Find(idHerramienta);
            if (herramienta == null)
                return (false, "Herramienta no encontrada.");

            var estadoActual = herramienta.EstadoHerramienta;

            // Validar que la transición sea permitida
            if (!TransicionPermitida(estadoActual, nuevoEstado))
                return (false, $"No se puede cambiar de '{estadoActual}' a '{nuevoEstado}'.");

            herramienta.EstadoHerramienta = nuevoEstado;
            _context.SaveChanges();

            return (true, $"Estado actualizado a '{nuevoEstado}'.");
        }

        // Verifica si la herramienta está disponible para reserva o alquiler
        public bool EstaDisponible(int idHerramienta)
        {
            var herramienta = _context.Herramienta.Find(idHerramienta);
            return herramienta?.EstadoHerramienta == Estados.Disponible;
        }

        // Verifica si la herramienta está reservada (para poder alquilarla)
        public bool EstaReservada(int idHerramienta)
        {
            var herramienta = _context.Herramienta.Find(idHerramienta);
            return herramienta?.EstadoHerramienta == Estados.Reservada;
        }

        // Tabla de transiciones permitidas según reglas de negocio del documento
        private bool TransicionPermitida(string? estadoActual, string nuevoEstado)
        {
            return (estadoActual, nuevoEstado) switch
            {
                // RN-08: reserva → Reservada
                (Estados.Disponible, Estados.Reservada) => true,
                // RN-09: alquiler directo o desde reserva → Alquilada
                (Estados.Disponible, Estados.Alquilada) => true,
                (Estados.Reservada, Estados.Alquilada) => true,
                // RN-11: devolución normal → Disponible
                (Estados.Alquilada, Estados.Disponible) => true,
                // RN-12: devolución con daños → Mantenimiento
                (Estados.Alquilada, Estados.Mantenimiento) => true,
                // Mantenimiento terminado → Disponible
                (Estados.Mantenimiento, Estados.Disponible) => true,
                // Admin cancela reserva → Disponible
                (Estados.Reservada, Estados.Disponible) => true,
                // Cualquier estado → Dada de baja (solo admin)
                (_, Estados.DadaDeBaja) => true,
                // Todo lo demás está bloqueado
                _ => false
            };
        }
    }
}
