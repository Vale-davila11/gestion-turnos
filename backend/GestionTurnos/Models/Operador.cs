using System.ComponentModel.DataAnnotations;

namespace GestionTurnos.Models;

public class Operador
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Contacto { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Rol { get; set; } = string.Empty;

    public ICollection<AsignacionTurno> Asignaciones { get; set; } = new List<AsignacionTurno>();
}
