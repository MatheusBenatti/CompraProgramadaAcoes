using Microsoft.EntityFrameworkCore;
using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<Cliente> Clientes { get; set; }
  public DbSet<ContaGrafica> ContasGraficas { get; set; }
  public DbSet<Custodia> Custodias { get; set; }
  public DbSet<HistoricoValorMensal> HistoricoValoresMensais { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Cliente
    modelBuilder.Entity<Cliente>(entity =>
    {
      entity.ToTable("Clientes");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).ValueGeneratedOnAdd();
      entity.Property(e => e.Nome).HasMaxLength(200).IsRequired();
      entity.Property(e => e.Cpf).HasMaxLength(11).IsRequired();
      entity.HasIndex(e => e.Cpf).IsUnique();
      entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
      entity.Property(e => e.ValorMensal).HasColumnType("decimal(18,2)").IsRequired();
      entity.Property(e => e.Ativo).HasDefaultValue(true);
      entity.Property(e => e.DataAdesao).IsRequired();
    });

    // ContaGrafica
    modelBuilder.Entity<ContaGrafica>(entity =>
    {
      entity.ToTable("ContasGraficas");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).ValueGeneratedOnAdd();
      entity.Property(e => e.NumeroConta).HasMaxLength(20);
      entity.HasIndex(e => e.NumeroConta).IsUnique();
      entity.Property(e => e.Tipo)
          .HasConversion<string>()
          .HasMaxLength(7)
          .IsRequired();
      entity.Property(e => e.DataCriacao).IsRequired();
      entity.HasOne(e => e.Cliente)
          .WithOne(e => e.ContaGrafica)
          .HasForeignKey<ContaGrafica>(e => e.ClienteId)
          .OnDelete(DeleteBehavior.Cascade)
          .HasConstraintName("FK_Conta_Cliente");
    });

    // Custodia
    modelBuilder.Entity<Custodia>(entity =>
    {
      entity.ToTable("Custodias");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).ValueGeneratedOnAdd();
      entity.Property(e => e.Ticker).HasMaxLength(10);
      entity.Property(e => e.Quantidade).IsRequired();
      entity.Property(e => e.PrecoMedio).HasColumnType("decimal(18,4)").IsRequired();
      entity.Property(e => e.DataUltimaAtualizacao).IsRequired();
      entity.HasOne(e => e.ContaGrafica)
          .WithMany(e => e.Custodias)
          .HasForeignKey(e => e.ContaGraficaId)
          .OnDelete(DeleteBehavior.Cascade)
          .HasConstraintName("FK_Custodia_Conta");
    });

    // HistoricoValorMensal
    modelBuilder.Entity<HistoricoValorMensal>(entity =>
    {
      entity.ToTable("HistoricoValoresMensais");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).ValueGeneratedOnAdd();
      entity.Property(e => e.ValorAnterior).HasColumnType("decimal(18,2)").IsRequired();
      entity.Property(e => e.ValorNovo).HasColumnType("decimal(18,2)").IsRequired();
      entity.Property(e => e.DataAlteracao).IsRequired();
      entity.HasIndex(e => e.ClienteId);
    });
  }
}