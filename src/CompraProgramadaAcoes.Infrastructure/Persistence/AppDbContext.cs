using Microsoft.EntityFrameworkCore;
using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<Cliente> Clientes { get; set; }
  public DbSet<ContaGrafica> ContasGraficas { get; set; }
  public DbSet<Custodia> Custodias { get; set; }
  public DbSet<HistoricoValorMensal> HistoricoValoresMensais { get; set; }
  public DbSet<OrdemCompra> OrdensCompra { get; set; }
  public DbSet<CestaRecomendacao> CestasRecomendacao { get; set; }
  public DbSet<ItemCesta> ItensCesta { get; set; }
  public DbSet<Distribuicao> Distribuicoes { get; set; }
  public DbSet<EventoIR> EventosIR { get; set; }
  public DbSet<Rebalanceamento> Rebalanceamentos { get; set; }
  public DbSet<Venda> Vendas { get; set; }
  public DbSet<Cotacao> Cotacoes { get; set; }
  public DbSet<CotacaoB3> CotacoesB3 { get; set; }

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
          .HasConstraintName("FK_Conta_Cliente")
          .IsRequired(false);
    });

    // Custodia
    modelBuilder.Entity<Custodia>(entity =>
    {
      entity.ToTable("Custodias");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).ValueGeneratedOnAdd();
      entity.Property(e => e.Ticker).HasMaxLength(10);
      entity.HasIndex(e => new { e.ContaGraficaId, e.Ticker })
      .IsUnique();
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
      entity.HasOne(e => e.Cliente)
          .WithMany()
          .HasForeignKey(e => e.ClienteId)
          .OnDelete(DeleteBehavior.Cascade)
          .HasConstraintName("FK_HistoricoValorMensal_Cliente");
    });

    // OrdemCompra
    modelBuilder.Entity<OrdemCompra>(entity =>
    {
      entity.ToTable("OrdensCompra");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).ValueGeneratedOnAdd();
      entity.Property(e => e.Ticker).HasMaxLength(10).IsRequired();
      entity.Property(e => e.Quantidade).IsRequired();
      entity.Property(e => e.PrecoUnitario).HasColumnType("decimal(18,4)").IsRequired();
      entity.Property(e => e.TipoMercado)
          .HasConversion<string>()
          .HasMaxLength(11)
          .IsRequired();
      entity.Property(e => e.DataExecucao).IsRequired();
      entity.HasOne(e => e.ContaMaster)
          .WithMany(e => e.OrdensCompra)
          .HasForeignKey(e => e.ContaMasterId)
          .OnDelete(DeleteBehavior.Restrict)
          .HasConstraintName("FK_OrdemCompra_ContaMaster");
    });

    // CestaRecomendacao
    modelBuilder.Entity<CestaRecomendacao>(entity =>
    {
      entity.ToTable("CestasRecomendacao");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).ValueGeneratedOnAdd();
      entity.Property(e => e.Nome).HasMaxLength(100).IsRequired();
      entity.Property(e => e.Ativa).HasDefaultValue(true);
      entity.Property(e => e.DataCriacao).IsRequired();
      entity.Property(e => e.DataDesativacao);
    });

    // ItemCesta
    modelBuilder.Entity<ItemCesta>(entity =>
    {
      entity.ToTable("ItensCesta");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).ValueGeneratedOnAdd();
      entity.Property(e => e.Ticker).HasMaxLength(10).IsRequired();
      entity.HasIndex(e => new { e.CestaId, e.Ticker })
        .IsUnique();
      entity.Property(e => e.Percentual).HasColumnType("decimal(5,2)").IsRequired();
      entity.HasOne(e => e.Cesta)
          .WithMany(e => e.Itens)
          .HasForeignKey(e => e.CestaId)
          .OnDelete(DeleteBehavior.Cascade)
          .HasConstraintName("FK_ItemCesta_Cesta");
    });

    // Distribuicao
    modelBuilder.Entity<Distribuicao>(entity =>
    {
      entity.ToTable("Distribuicoes");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).ValueGeneratedOnAdd();
      entity.Property(e => e.Ticker).HasMaxLength(10).IsRequired();
      entity.Property(e => e.Quantidade).IsRequired();
      entity.Property(e => e.PrecoUnitario).HasColumnType("decimal(18,4)").IsRequired();
      entity.Property(e => e.DataDistribuicao).IsRequired();
      entity.HasOne(e => e.OrdemCompra)
          .WithMany(e => e.Distribuicoes)
          .HasForeignKey(e => e.OrdemCompraId)
          .OnDelete(DeleteBehavior.Cascade)
          .HasConstraintName("FK_Distribuicao_OrdemCompra");
      entity.HasOne(e => e.CustodiaFilhote)
          .WithMany()
          .HasForeignKey(e => e.CustodiaFilhoteId)
          .OnDelete(DeleteBehavior.Cascade)
          .HasConstraintName("FK_Distribuicao_Custodia");
    });

    // EventoIR
    modelBuilder.Entity<EventoIR>(entity =>
    {
      entity.ToTable("EventosIR");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).ValueGeneratedOnAdd();
      entity.Property(e => e.Tipo)
          .HasConversion<string>()
          .HasMaxLength(10)
          .IsRequired();
      entity.Property(e => e.ValorBase).HasColumnType("decimal(18,2)").IsRequired();
      entity.Property(e => e.ValorIR).HasColumnType("decimal(18,2)").IsRequired();
      entity.Property(e => e.PublicadoKafka).HasDefaultValue(false);
      entity.Property(e => e.DataEvento).IsRequired();
      entity.HasOne(e => e.Cliente)
          .WithMany()
          .HasForeignKey(e => e.ClienteId)
          .OnDelete(DeleteBehavior.Cascade)
          .HasConstraintName("FK_EventoIR_Cliente");
    });

    // Rebalanceamento
    modelBuilder.Entity<Rebalanceamento>(entity =>
    {
      entity.ToTable("Rebalanceamentos");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).ValueGeneratedOnAdd();
      entity.Property(e => e.Tipo)
          .HasConversion<string>()
          .HasMaxLength(15)
          .IsRequired();
      entity.Property(e => e.TickerVendido).HasMaxLength(10).IsRequired();
      entity.Property(e => e.TickerComprado).HasMaxLength(10).IsRequired();
      entity.Property(e => e.ValorVenda).HasColumnType("decimal(18,2)").IsRequired();
      entity.Property(e => e.DataRebalanceamento).IsRequired();
      entity.HasOne(e => e.Cliente)
          .WithMany()
          .HasForeignKey(e => e.ClienteId)
          .OnDelete(DeleteBehavior.Cascade)
          .HasConstraintName("FK_Rebalanceamento_Cliente");
    });

    // Venda
    modelBuilder.Entity<Venda>(entity =>
    {
      entity.ToTable("Vendas");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).ValueGeneratedOnAdd();
      entity.Property(e => e.Ticker).HasMaxLength(10).IsRequired();
      entity.Property(e => e.Quantidade).IsRequired();
      entity.Property(e => e.ValorVenda).HasColumnType("decimal(18,2)").IsRequired();
      entity.Property(e => e.PrecoUnitario).HasColumnType("decimal(18,4)").IsRequired();
      entity.Property(e => e.DataOperacao).IsRequired();
      entity.Property(e => e.CustoAquisicao).HasColumnType("decimal(18,2)").IsRequired();
      entity.Property(e => e.Lucro).HasColumnType("decimal(18,2)").IsRequired();
      entity.Property(e => e.IrRetido).HasColumnType("decimal(18,2)").IsRequired();
    });

    // Cotacao
    modelBuilder.Entity<Cotacao>(entity =>
    {
      entity.ToTable("Cotacoes");
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Id).ValueGeneratedOnAdd();
      entity.Property(e => e.DataPregao).IsRequired();
      entity.Property(e => e.Ticker).HasMaxLength(10).IsRequired();
      entity.Property(e => e.PrecoAbertura).HasColumnType("decimal(18,4)").IsRequired();
      entity.Property(e => e.PrecoFechamento).HasColumnType("decimal(18,4)").IsRequired();
      entity.Property(e => e.PrecoMaximo).HasColumnType("decimal(18,4)").IsRequired();
      entity.Property(e => e.PrecoMinimo).HasColumnType("decimal(18,4)").IsRequired();
      entity.HasIndex(e => new { e.DataPregao, e.Ticker }).IsUnique();
    });

    // CotacaoB3
    modelBuilder.Entity<CotacaoB3>(entity =>
    {
      entity.ToTable("CotacoesB3");
      entity.HasKey(e => new { e.DataPregao, e.Ticker });
      entity.Property(e => e.Ticker).HasMaxLength(10).IsRequired();
      entity.Property(e => e.CodigoBDI).HasMaxLength(10).IsRequired();
      entity.Property(e => e.TipoMercado).IsRequired();
      entity.Property(e => e.NomeEmpresa).HasMaxLength(200).IsRequired();
      entity.Property(e => e.PrecoAbertura).HasColumnType("decimal(18,4)").IsRequired();
      entity.Property(e => e.PrecoMaximo).HasColumnType("decimal(18,4)").IsRequired();
      entity.Property(e => e.PrecoMinimo).HasColumnType("decimal(18,4)").IsRequired();
      entity.Property(e => e.PrecoFechamento).HasColumnType("decimal(18,4)").IsRequired();
      entity.Property(e => e.PrecoMedio).HasColumnType("decimal(18,4)").IsRequired();
      entity.Property(e => e.QuantidadeNegociada).IsRequired();
      entity.Property(e => e.VolumeNegociado).HasColumnType("decimal(18,2)").IsRequired();
    });
  }
}