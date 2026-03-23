using System.ComponentModel.DataAnnotations;

namespace LoginNet.Models;

public class Usuario
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string NumeroDocumento { get; set; } = string.Empty;

    [Required]
    [StringLength(3)]
    public string TipoDocumento { get; set; } = "DNI";

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Apellido { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Correo { get; set; } = string.Empty;

    [Required]
    public string ClaveHash { get; set; } = string.Empty;

    public int CVF { get; set; } = 0;

    public DateTime? FechaBloqueo { get; set; }

    public bool EstaBloqueado => FechaBloqueo.HasValue && FechaBloqueo.Value > DateTime.Now;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public bool Activo { get; set; } = true;
}
