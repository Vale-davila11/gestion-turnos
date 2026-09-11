using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionTurnos.Data;
using GestionTurnos.Models;

namespace GestionTurnos.Controllers;

[Authorize]
public class AsignacionesController : Controller
{
    private readonly ApplicationDbContext _context;

    public AsignacionesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(DateOnly? semana)
    {
        var inicioSemana = ObtenerLunesDeLaSemana(semana ?? DateOnly.FromDateTime(DateTime.Today));
        var dias = Enumerable.Range(0, 7).Select(inicioSemana.AddDays).ToList();
        var finSemana = dias[^1];

        var tiposTurno = await _context.TiposTurno.OrderBy(t => t.HoraInicio).ToListAsync();
        var operadores = await _context.Operadores.OrderBy(o => o.Nombre).ToListAsync();

        var asignacionesExistentes = await _context.AsignacionesTurno
            .Where(a => a.Fecha >= inicioSemana && a.Fecha <= finSemana)
            .ToListAsync();

        var celdas = new List<CeldaAsignacionViewModel>();
        foreach (var tipoTurno in tiposTurno)
        {
            foreach (var dia in dias)
            {
                var existente = asignacionesExistentes
                    .FirstOrDefault(a => a.TipoTurnoId == tipoTurno.Id && a.Fecha == dia);

                celdas.Add(new CeldaAsignacionViewModel
                {
                    TipoTurnoId = tipoTurno.Id,
                    Fecha = dia,
                    OperadorId = existente?.OperadorId
                });
            }
        }

        var viewModel = new AsignacionSemanalViewModel
        {
            InicioSemana = inicioSemana,
            Dias = dias,
            TiposTurno = tiposTurno,
            Operadores = operadores,
            Celdas = celdas
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Guardar(List<CeldaAsignacionViewModel> celdas, DateOnly semana)
    {
        var fechas = celdas.Select(c => c.Fecha).Distinct().ToList();
        var existentes = await _context.AsignacionesTurno
            .Where(a => fechas.Contains(a.Fecha))
            .ToListAsync();

        foreach (var celda in celdas)
        {
            var existente = existentes.FirstOrDefault(
                a => a.Fecha == celda.Fecha && a.TipoTurnoId == celda.TipoTurnoId);

            if (celda.OperadorId.HasValue)
            {
                if (existente == null)
                {
                    _context.AsignacionesTurno.Add(new AsignacionTurno
                    {
                        Fecha = celda.Fecha,
                        TipoTurnoId = celda.TipoTurnoId,
                        OperadorId = celda.OperadorId.Value
                    });
                }
                else if (existente.OperadorId != celda.OperadorId.Value)
                {
                    existente.OperadorId = celda.OperadorId.Value;
                }
            }
            else if (existente != null)
            {
                _context.AsignacionesTurno.Remove(existente);
            }
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { semana = semana.ToString("yyyy-MM-dd") });
    }

    private static DateOnly ObtenerLunesDeLaSemana(DateOnly fecha)
    {
        var diferencia = ((int)fecha.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return fecha.AddDays(-diferencia);
    }
}
