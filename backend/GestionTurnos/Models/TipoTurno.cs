using System.ComponentModel.DataAnnotations;

namespace GestionTurnos.Models;

public class TipoTurno
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public TimeOnly HoraInicio { get; set; }

    [Required]
    public TimeOnly HoraFin { get; set; }

    public ICollection<AsignacionTurno> Asignaciones { get; set; } = new List<AsignacionTurno>();
}
