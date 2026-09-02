using Microsoft.EntityFrameworkCore;
using GestionTurnos.Models;

namespace GestionTurnos.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Operador> Operadores => Set<Operador>();
    public DbSet<TipoTurno> TiposTurno => Set<TipoTurno>();
    public DbSet<AsignacionTurno> AsignacionesTurno => Set<AsignacionTurno>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AsignacionTurno>()
            .HasIndex(a => new { a.OperadorId, a.Fecha, a.TipoTurnoId })
            .IsUnique();
    }
}
