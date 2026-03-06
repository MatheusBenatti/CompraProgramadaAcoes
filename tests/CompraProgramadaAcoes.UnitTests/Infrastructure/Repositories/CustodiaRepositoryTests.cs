using CompraProgramadaAcoes.Infrastructure.Repositories;
using CompraProgramadaAcoes.Infrastructure.Persistence;
using CompraProgramadaAcoes.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore.InMemory;

namespace CompraProgramadaAcoes.UnitTests.Infrastructure.Repositories;

public class CustodiaRepositoryTests
{
    private readonly AppDbContext _context;
    private readonly CustodiaRepository _repository;

    public CustodiaRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _repository = new CustodiaRepository(_context);
    }

    [Fact]
    public async Task ObterPorContaGraficaAsync_ContaGraficaIdValido_DeveRetornarCustodias()
    {
        // Arrange
        var contaGrafica = new ContaGrafica(1);
        await _context.ContasGraficas.AddAsync(contaGrafica);
        await _context.SaveChangesAsync();
        
        var custodia1 = new Custodia(contaGrafica.Id);
        var custodia2 = new Custodia(contaGrafica.Id);
        custodia1.AtualizarCustodia(100, 25m, "PETR4");
        custodia2.AtualizarCustodia(50, 15m, "VALE3");
        
        await _context.Custodias.AddRangeAsync(custodia1, custodia2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterPorContaGraficaAsync(contaGrafica.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Ticker == "PETR4");
        result.Should().Contain(c => c.Ticker == "VALE3");
    }

    [Fact]
    public async Task ObterPorContaMasterAsync_ContaMasterIdValido_DeveRetornarCustodias()
    {
        // Arrange
        var contaGrafica = new ContaGrafica(1);
        await _context.ContasGraficas.AddAsync(contaGrafica);
        await _context.SaveChangesAsync();
        
        var custodia1 = new Custodia(contaGrafica.Id);
        var custodia2 = new Custodia(contaGrafica.Id);
        custodia1.AtualizarCustodia(100, 25m, "PETR4");
        custodia2.AtualizarCustodia(50, 15m, "VALE3");
        
        await _context.Custodias.AddRangeAsync(custodia1, custodia2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterPorContaMasterAsync(contaGrafica.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Ticker == "PETR4");
        result.Should().Contain(c => c.Ticker == "VALE3");
    }

    [Fact]
    public async Task ObterPorTickerAsync_ContaGraficaIdETickerValidos_DeveRetornarCustodia()
    {
        // Arrange
        var contaGrafica = new ContaGrafica(1);
        await _context.ContasGraficas.AddAsync(contaGrafica);
        await _context.SaveChangesAsync();
        
        var custodia = new Custodia(contaGrafica.Id);
        custodia.AtualizarCustodia(100, 25m, "PETR4");
        await _context.Custodias.AddAsync(custodia);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterPorTickerAsync(contaGrafica.Id, "PETR4");

        // Assert
        result.Should().NotBeNull();
        result.Ticker.Should().Be("PETR4");
        result.Quantidade.Should().Be(100);
    }

    [Fact]
    public async Task ObterPorTickerAsync_TickerNaoEncontrado_DeveRetornarNulo()
    {
        // Arrange
        var contaGrafica = new ContaGrafica(1);
        await _context.ContasGraficas.AddAsync(contaGrafica);
        await _context.SaveChangesAsync();
        
        var custodia = new Custodia(contaGrafica.Id);
        custodia.AtualizarCustodia(100, 25m, "PETR4");
        await _context.Custodias.AddAsync(custodia);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterPorTickerAsync(contaGrafica.Id, "VALE3");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ObterPorTickerMasterAsync_ContaMasterIdETickerValidos_DeveRetornarCustodia()
    {
        // Arrange
        var contaGrafica = new ContaGrafica(1);
        await _context.ContasGraficas.AddAsync(contaGrafica);
        await _context.SaveChangesAsync();
        
        var custodia = new Custodia(contaGrafica.Id);
        custodia.AtualizarCustodia(100, 25m, "PETR4");
        await _context.Custodias.AddAsync(custodia);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterPorTickerMasterAsync(contaGrafica.Id, "PETR4");

        // Assert
        result.Should().NotBeNull();
        result.Ticker.Should().Be("PETR4");
        result.Quantidade.Should().Be(100);
    }

    [Fact]
    public async Task AddAsync_CustodiaValida_DeveAdicionarCustodia()
    {
        // Arrange
        var contaGrafica = new ContaGrafica(1);
        await _context.ContasGraficas.AddAsync(contaGrafica);
        await _context.SaveChangesAsync();
        
        var custodia = new Custodia(contaGrafica.Id);
        custodia.AtualizarCustodia(100, 25m, "PETR4");

        // Act
        await _repository.AddAsync(custodia);
        await _repository.SaveChangesAsync(); // Salva as mudanças

        // Assert
        // Verifica se foi adicionado ao contexto
        var custodias = await _context.Custodias.ToListAsync();
        custodias.Should().Contain(custodia);
    }

    [Fact]
    public async Task UpdateAsync_CustodiaValida_DeveAtualizarCustodia()
    {
        // Arrange
        var contaGrafica = new ContaGrafica(1);
        await _context.ContasGraficas.AddAsync(contaGrafica);
        await _context.SaveChangesAsync();
        
        var custodia = new Custodia(contaGrafica.Id);
        custodia.AtualizarCustodia(100, 25m, "PETR4");
        await _context.Custodias.AddAsync(custodia);
        await _context.SaveChangesAsync();

        // Act
        await _repository.UpdateAsync(custodia);
        await _repository.SaveChangesAsync();

        // Assert - Verifica se a custódia foi atualizada (não lança exceção)
        var custodias = await _context.Custodias.ToListAsync();
        custodias.Should().Contain(custodia);
    }

    [Fact]
    public async Task SaveChangesAsync_DeveChamarSaveChangesDoContexto()
    {
        // Arrange
        var contaGrafica = new ContaGrafica(1);
        await _context.ContasGraficas.AddAsync(contaGrafica);
        await _context.SaveChangesAsync();
        
        var custodia = new Custodia(contaGrafica.Id);
        custodia.AtualizarCustodia(100, 25m, "PETR4");
        await _context.Custodias.AddAsync(custodia);

        // Act
        var result = await _repository.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        
        // Verifica se a custódia foi salva no banco
        var custodias = await _context.Custodias.ToListAsync();
        custodias.Should().HaveCount(1);
        custodias.First().Should().Be(custodia);
    }
}
