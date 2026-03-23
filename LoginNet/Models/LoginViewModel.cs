using System.ComponentModel.DataAnnotations;

namespace LoginNet.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Seleccione un tipo de documento")]
    public string TipoDocumento { get; set; } = "DNI";

    [Required(ErrorMessage = "El usuario es requerido")]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    [DataType(DataType.Password)]
    public string Contraseña { get; set; } = string.Empty;

    public bool Recordar { get; set; }
}
