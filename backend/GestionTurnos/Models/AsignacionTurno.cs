using System.ComponentModel.DataAnnotations;

namespace GestionTurnos.Models;

public class AsignacionTurno
{
    public int Id { get; set; }

    [Required]
    public DateOnly Fecha { get; set; }

    [Required]
    public int OperadorId { get; set; }
    public Operador? Operador { get; set; }

    [Required]
    public int TipoTurnoId { get; set; }
    public TipoTurno? TipoTurno { get; set; }
}
