using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionTurnos.Data;
using GestionTurnos.Models;

namespace GestionTurnos.Controllers;

[Authorize]
public class TiposTurnoController : Controller
{
    private readonly ApplicationDbContext _context;

    public TiposTurnoController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.TiposTurno.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var tipoTurno = await _context.TiposTurno.FirstOrDefaultAsync(t => t.Id == id);
        if (tipoTurno == null)
        {
            return NotFound();
        }

        return View(tipoTurno);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombre,HoraInicio,HoraFin")] TipoTurno tipoTurno)
    {
        if (ModelState.IsValid)
        {
            _context.Add(tipoTurno);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(tipoTurno);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var tipoTurno = await _context.TiposTurno.FindAsync(id);
        if (tipoTurno == null)
        {
            return NotFound();
        }

        return View(tipoTurno);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,HoraInicio,HoraFin")] TipoTurno tipoTurno)
    {
        if (id != tipoTurno.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(tipoTurno);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.TiposTurno.AnyAsync(t => t.Id == tipoTurno.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        return View(tipoTurno);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var tipoTurno = await _context.TiposTurno.FirstOrDefaultAsync(t => t.Id == id);
        if (tipoTurno == null)
        {
            return NotFound();
        }

        return View(tipoTurno);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var tipoTurno = await _context.TiposTurno.FindAsync(id);
        if (tipoTurno != null)
        {
            _context.TiposTurno.Remove(tipoTurno);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
