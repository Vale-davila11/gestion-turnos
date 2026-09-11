namespace GestionTurnos.Models;

public class AsignacionSemanalViewModel
{
    public DateOnly InicioSemana { get; set; }
    public List<DateOnly> Dias { get; set; } = new();
    public List<TipoTurno> TiposTurno { get; set; } = new();
    public List<Operador> Operadores { get; set; } = new();
    public List<CeldaAsignacionViewModel> Celdas { get; set; } = new();
}

public class CeldaAsignacionViewModel
{
    public int TipoTurnoId { get; set; }
    public DateOnly Fecha { get; set; }
    public int? OperadorId { get; set; }
}
