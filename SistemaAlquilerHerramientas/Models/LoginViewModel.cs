using System.ComponentModel.DataAnnotations;

namespace SistemaAlquilerHerramientas.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El correo o usuario es obligatorio")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contrasena es obligatoria")]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; } = string.Empty;
    }
}
