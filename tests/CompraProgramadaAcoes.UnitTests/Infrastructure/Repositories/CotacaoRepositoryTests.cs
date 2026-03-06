using CompraProgramadaAcoes.Infrastructure.Repositories;
using CompraProgramadaAcoes.Infrastructure.Persistence;
using CompraProgramadaAcoes.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore.InMemory;

namespace CompraProgramadaAcoes.UnitTests.Infrastructure.Repositories;

public class CotacaoRepositoryTests
{
    private readonly AppDbContext _context;
    private readonly CotacaoRepository _repository;

    public CotacaoRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _repository = new CotacaoRepository(_context);
    }

    [Fact]
    public async Task BulkInsertAsync_CotacoesValidas_DeveAdicionarCotacoes()
    {
        // Arrange
        var cotacoes = new List<Cotacao>
        {
            new Cotacao { Ticker = "PETR4", DataPregao = DateTime.Today, PrecoAbertura = 10.5m, PrecoFechamento = 11.0m },
            new Cotacao { Ticker = "VALE3", DataPregao = DateTime.Today, PrecoAbertura = 15.0m, PrecoFechamento = 15.5m }
        };

        // Act
        await _repository.BulkInsertAsync(cotacoes);

        // Assert
        var cotacoesSalvas = await _context.Cotacoes.ToListAsync();
        cotacoesSalvas.Should().HaveCount(2);
        cotacoesSalvas.Should().Contain(c => c.Ticker == "PETR4");
        cotacoesSalvas.Should().Contain(c => c.Ticker == "VALE3");
    }

    [Fact]
    public async Task BulkInsertAsync_CotacoesNulasOuVazias_NaoDeveFazerNada()
    {
        // Act
        await _repository.BulkInsertAsync(null);
        await _repository.BulkInsertAsync(new List<Cotacao>());

        // Assert
        var cotacoesSalvas = await _context.Cotacoes.ToListAsync();
        cotacoesSalvas.Should().BeEmpty();
    }

    [Fact]
    public async Task BulkInsertAsync_ComCotacoesExistentes_DeveRemoverExistentesEAdicionarNovas()
    {
        // Arrange
        var cotacoesExistentes = new List<Cotacao>
        {
            new Cotacao { Ticker = "PETR4", DataPregao = DateTime.Today, PrecoAbertura = 9.5m, PrecoFechamento = 10.0m }
        };
        await _context.Cotacoes.AddRangeAsync(cotacoesExistentes);
        await _context.SaveChangesAsync();
        
        var cotacoesNovas = new List<Cotacao>
        {
            new Cotacao { Ticker = "PETR4", DataPregao = DateTime.Today, PrecoAbertura = 10.5m, PrecoFechamento = 11.0m }
        };

        // Act
        await _repository.BulkInsertAsync(cotacoesNovas);

        // Assert
        var cotacoesSalvas = await _context.Cotacoes.ToListAsync();
        cotacoesSalvas.Should().HaveCount(1);
        cotacoesSalvas.First().PrecoAbertura.Should().Be(10.5m);
        cotacoesSalvas.First().PrecoFechamento.Should().Be(11.0m);
    }

    [Fact]
    public async Task ObterPorDataAsync_DataValida_DeveRetornarCotacoesOrdenadasPorTicker()
    {
        // Arrange
        var cotacoes = new List<Cotacao>
        {
            new Cotacao { Ticker = "VALE3", DataPregao = DateTime.Today, PrecoAbertura = 15.0m, PrecoFechamento = 15.5m },
            new Cotacao { Ticker = "PETR4", DataPregao = DateTime.Today, PrecoAbertura = 10.5m, PrecoFechamento = 11.0m }
        };
        await _context.Cotacoes.AddRangeAsync(cotacoes);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterPorDataAsync(DateTime.Today);

        // Assert
        result.Should().HaveCount(2);
        result.First().Ticker.Should().Be("PETR4"); // Ordenado por ticker
        result.Last().Ticker.Should().Be("VALE3");
    }

    [Fact]
    public async Task ObterPorTickerEDataAsync_TickerEDataValidos_DeveRetornarCotacao()
    {
        // Arrange
        var cotacoes = new List<Cotacao>
        {
            new Cotacao { Ticker = "PETR4", DataPregao = DateTime.Today, PrecoAbertura = 10.5m, PrecoFechamento = 11.0m },
            new Cotacao { Ticker = "VALE3", DataPregao = DateTime.Today, PrecoAbertura = 15.0m, PrecoFechamento = 15.5m }
        };
        await _context.Cotacoes.AddRangeAsync(cotacoes);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterPorTickerEDataAsync("PETR4", DateTime.Today);

        // Assert
        result.Should().NotBeNull();
        result.Ticker.Should().Be("PETR4");
    }

    [Fact]
    public async Task ObterPorTickersEDataAsync_MultiplosTickers_DeveRetornarCotacoesOrdenadasPorTicker()
    {
        // Arrange
        var cotacoes = new List<Cotacao>
        {
            new Cotacao { Ticker = "VALE3", DataPregao = DateTime.Today, PrecoAbertura = 15.0m, PrecoFechamento = 15.5m },
            new Cotacao { Ticker = "PETR4", DataPregao = DateTime.Today, PrecoAbertura = 10.5m, PrecoFechamento = 11.0m },
            new Cotacao { Ticker = "ITUB4", DataPregao = DateTime.Today, PrecoAbertura = 20.0m, PrecoFechamento = 20.5m }
        };
        await _context.Cotacoes.AddRangeAsync(cotacoes);
        await _context.SaveChangesAsync();

        var tickers = new[] { "PETR4", "VALE3" };

        // Act
        var result = await _repository.ObterPorTickersEDataAsync(tickers, DateTime.Today);

        // Assert
        result.Should().HaveCount(2);
        result.First().Ticker.Should().Be("PETR4"); // Ordenado por ticker
        result.Last().Ticker.Should().Be("VALE3");
    }
}
