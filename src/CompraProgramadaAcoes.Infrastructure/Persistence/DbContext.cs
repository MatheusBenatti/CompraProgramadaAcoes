using Microsoft.EntityFrameworkCore;
using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<Product> Products { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<ContaGrafica> ContasGraficas { get; set; }
    public DbSet<Custodia> Custodias { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cliente
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Cpf).IsRequired().HasMaxLength(11);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ValorMensal).HasPrecision(18, 2);
            entity.HasIndex(e => e.Cpf).IsUnique();
        });

        // ContaGrafica
        modelBuilder.Entity<ContaGrafica>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NumeroConta).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Tipo).IsRequired().HasMaxLength(20);
            entity.HasOne(e => e.Cliente)
                  .WithOne(c => c.ContaGrafica)
                  .HasForeignKey<ContaGrafica>(e => e.ClienteId);
        });

        // Custodia
        modelBuilder.Entity<Custodia>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ValorTotal).HasPrecision(18, 2);
            entity.HasOne(e => e.Cliente)
                  .WithOne(c => c.Custodia)
                  .HasForeignKey<Custodia>(e => e.ClienteId);
            entity.HasOne(e => e.ContaGrafica)
                  .WithMany()
                  .HasForeignKey(e => e.ContaGraficaId);
        });
    }
}