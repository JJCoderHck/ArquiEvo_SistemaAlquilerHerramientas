using System.ComponentModel.DataAnnotations;

namespace SistemaAlquilerHerramientas.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Ingresa tus nombres.")]
    [StringLength(100, ErrorMessage = "Los nombres no deben superar 100 caracteres.")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa tus apellidos.")]
    [StringLength(100, ErrorMessage = "Los apellidos no deben superar 100 caracteres.")]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa tu DNI.")]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "El DNI debe tener 8 digitos.")]
    [RegularExpression("^[0-9]{8}$", ErrorMessage = "El DNI solo debe contener numeros.")]
    public string Dni { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "El telefono no debe superar 20 caracteres.")]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "Ingresa tu correo.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo valido.")]
    [StringLength(150, ErrorMessage = "El correo no debe superar 150 caracteres.")]
    public string Correo { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "La direccion no debe superar 255 caracteres.")]
    public string? Direccion { get; set; }

    [Required(ErrorMessage = "Ingresa un nombre de usuario.")]
    [StringLength(50, ErrorMessage = "El usuario no debe superar 50 caracteres.")]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa una contrasena.")]
    [StringLength(255, MinimumLength = 6, ErrorMessage = "La contrasena debe tener al menos 6 caracteres.")]
    [DataType(DataType.Password)]
    public string Contrasena { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirma la contrasena.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Contrasena), ErrorMessage = "Las contrasenas no coinciden.")]
    public string ConfirmarContrasena { get; set; } = string.Empty;
}
