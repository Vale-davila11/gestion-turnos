using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionTurnos.Data;
using GestionTurnos.Models;

namespace GestionTurnos.Controllers;

[Authorize]
public class OperadoresController : Controller
{
    private readonly ApplicationDbContext _context;

    public OperadoresController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Operadores.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var operador = await _context.Operadores.FirstOrDefaultAsync(o => o.Id == id);
        if (operador == null)
        {
            return NotFound();
        }

        return View(operador);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nombre,Contacto,Rol")] Operador operador)
    {
        if (ModelState.IsValid)
        {
            _context.Add(operador);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(operador);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var operador = await _context.Operadores.FindAsync(id);
        if (operador == null)
        {
            return NotFound();
        }

        return View(operador);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Contacto,Rol")] Operador operador)
    {
        if (id != operador.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(operador);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Operadores.AnyAsync(o => o.Id == operador.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        return View(operador);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var operador = await _context.Operadores.FirstOrDefaultAsync(o => o.Id == id);
        if (operador == null)
        {
            return NotFound();
        }

        return View(operador);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var operador = await _context.Operadores.FindAsync(id);
        if (operador != null)
        {
            _context.Operadores.Remove(operador);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
