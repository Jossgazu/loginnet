using System.ComponentModel.DataAnnotations;

namespace LoginNet.Models;

public class RegistrarViewModel
{
    [Required(ErrorMessage = "El número de documento es requerido")]
    [StringLength(20)]
    public string NumeroDocumento { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de documento es requerido")]
    [StringLength(3)]
    public string TipoDocumento { get; set; } = "DNI";

    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es requerido")]
    [StringLength(100)]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es requerido")]
    [EmailAddress(ErrorMessage = "El correo no es válido")]
    [StringLength(100)]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string Contraseña { get; set; } = string.Empty;
}
