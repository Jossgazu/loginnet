using Microsoft.EntityFrameworkCore;
using LoginNet.Models;

namespace LoginNet.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(e => e.NumeroDocumento).IsUnique();
            entity.HasIndex(e => e.Correo).IsUnique();
        });
    }
}
