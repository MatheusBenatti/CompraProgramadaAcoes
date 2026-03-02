using CompraProgramadaAcoes.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CompraProgramadaAcoes.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets para todas as entidades
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<ContaGrafica> ContasGraficas { get; set; }
    public DbSet<Custodia> Custodias { get; set; }
    public DbSet<OrdemCompra> OrdensCompra { get; set; }
    public DbSet<Distribuicao> Distribuicoes { get; set; }
    public DbSet<CestaRecomendacao> CestasRecomendacao { get; set; }
    public DbSet<ItemCesta> ItensCesta { get; set; }
    public DbSet<Cotacao> Cotacoes { get; set; }
    public DbSet<Rebalanceamento> Rebalanceamentos { get; set; }
    public DbSet<EventoIR> EventosIR { get; set; }
    public DbSet<HistoricoValorMensal> HistoricosValorMensal { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cliente
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Cpf).IsRequired().HasMaxLength(11);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ValorMensal).HasPrecision(18, 2);
            entity.HasIndex(e => e.Cpf).IsUnique();
            entity.HasOne(e => e.ContaGrafica).WithOne(c => c.Cliente).HasForeignKey<ContaGrafica>(c => c.ClienteId);
        });

        // ContaGrafica
        modelBuilder.Entity<ContaGrafica>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.NumeroConta).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Tipo).IsRequired().HasMaxLength(10);
            entity.HasIndex(e => e.NumeroConta).IsUnique();
        });

        // Custodia
        modelBuilder.Entity<Custodia>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Ticker).IsRequired().HasMaxLength(10);
            entity.Property(e => e.PrecoMedio).HasPrecision(18, 4);
            entity.HasOne(e => e.ContaGrafica).WithMany(cg => cg.Custodias).HasForeignKey(e => e.ContaGraficaId);
        });

        // OrdemCompra
        modelBuilder.Entity<OrdemCompra>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Ticker).IsRequired().HasMaxLength(10);
            entity.Property(e => e.PrecoUnitario).HasPrecision(18, 4);
            entity.Property(e => e.TipoMercado).IsRequired().HasMaxLength(20);
            entity.HasOne(e => e.ContaMaster).WithMany().HasForeignKey(e => e.ContaMasterId);
        });

        // Distribuicao
        modelBuilder.Entity<Distribuicao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Ticker).IsRequired().HasMaxLength(10);
            entity.Property(e => e.PrecoUnitario).HasPrecision(18, 4);
            entity.HasOne(e => e.OrdemCompra).WithMany(o => o.Distribuicoes).HasForeignKey(e => e.OrdemCompraId);
            entity.HasOne(e => e.CustodiaFilhote).WithMany().HasForeignKey(e => e.CustodiaFilhoteId);
        });

        // CestaRecomendacao
        modelBuilder.Entity<CestaRecomendacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
        });

        // ItemCesta
        modelBuilder.Entity<ItemCesta>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Ticker).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Percentual).HasPrecision(5, 2);
            entity.HasOne(e => e.Cesta).WithMany(c => c.Itens).HasForeignKey(e => e.CestaId);
        });

        // Cotacao
        modelBuilder.Entity<Cotacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Ticker).IsRequired().HasMaxLength(10);
            entity.Property(e => e.PrecoAbertura).HasPrecision(18, 4);
            entity.Property(e => e.PrecoFechamento).HasPrecision(18, 4);
            entity.Property(e => e.PrecoMaximo).HasPrecision(18, 4);
            entity.Property(e => e.PrecoMinimo).HasPrecision(18, 4);
        });

        // Rebalanceamento
        modelBuilder.Entity<Rebalanceamento>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.TickerVendido).IsRequired().HasMaxLength(10);
            entity.Property(e => e.TickerComprado).IsRequired().HasMaxLength(10);
            entity.Property(e => e.ValorVenda).HasPrecision(18, 2);
            entity.HasOne(e => e.Cliente).WithMany().HasForeignKey(e => e.ClienteId);
        });

        // EventoIR
        modelBuilder.Entity<EventoIR>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ValorBase).HasPrecision(18, 2);
            entity.Property(e => e.ValorIR).HasPrecision(18, 2);
            entity.HasOne(e => e.Cliente).WithMany().HasForeignKey(e => e.ClienteId);
        });

        // HistoricoValorMensal
        modelBuilder.Entity<HistoricoValorMensal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ValorAnterior).HasPrecision(18, 2);
            entity.Property(e => e.ValorNovo).HasPrecision(18, 2);
        });

        // Configurar enums para strings
        modelBuilder.Entity<OrdemCompra>()
            .Property(e => e.TipoMercado)
            .HasConversion<string>();

        modelBuilder.Entity<Rebalanceamento>()
            .Property(e => e.Tipo)
            .HasConversion<string>();

        modelBuilder.Entity<EventoIR>()
            .Property(e => e.Tipo)
            .HasConversion<string>();
    }
}
