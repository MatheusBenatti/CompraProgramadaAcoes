using CompraProgramadaAcoes.Infrastructure.Repositories;
using CompraProgramadaAcoes.Infrastructure.Persistence;
using CompraProgramadaAcoes.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore.InMemory;

namespace CompraProgramadaAcoes.UnitTests.Infrastructure.Repositories;

public class ContaMasterRepositoryTests
{
    private readonly AppDbContext _context;
    private readonly ContaMasterRepository _repository;

    public ContaMasterRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _repository = new ContaMasterRepository(_context);
    }

    [Fact]
    public async Task ObterContaMasterAsync_ContaExistente_DeveRetornarConta()
    {
        // Arrange
        var contaMaster = new ContaGrafica(null!);
        contaMaster.AtualizarTipo("MASTER");
        contaMaster.AtualizarNumeroConta("MST-000001");
        
        await _context.ContasGraficas.AddAsync(contaMaster);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterContaMasterAsync();

        // Assert
        result.Should().NotBeNull();
        result.Tipo.Should().Be("MASTER");
    }

    [Fact]
    public async Task ObterContaMasterAsync_ContaNaoExistente_DeveCriarERetornarConta()
    {
        // Arrange - Sem contas no banco

        // Act
        var result = await _repository.ObterContaMasterAsync();

        // Assert
        result.Should().NotBeNull();
        result.Tipo.Should().Be("MASTER");
        result.NumeroConta.Should().Be("MST-000001");
        
        // Verifica se a conta foi criada no banco
        var contas = await _context.ContasGraficas.ToListAsync();
        contas.Should().HaveCount(1);
        contas.First().Tipo.Should().Be("MASTER");
    }

    [Fact]
    public async Task ObterComCustodiasAsync_IdValido_DeveRetornarContaComCustodias()
    {
        // Arrange
        var contaMaster = new ContaGrafica(null!);
        contaMaster.AtualizarTipo("MASTER");
        contaMaster.AtualizarNumeroConta("MST-000001");
        
        await _context.ContasGraficas.AddAsync(contaMaster);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterComCustodiasAsync(contaMaster.Id);

        // Assert
        result.Should().NotBeNull();
        result.Tipo.Should().Be("MASTER");
    }

    [Fact]
    public async Task ObterComCustodiasAsync_IdInvalido_DeveLancarExcecao()
    {
        // Arrange - Sem contas no banco

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.ObterComCustodiasAsync(999));
    }

    [Fact]
    public async Task ObterResiduosAsync_DeveRetornarCustodiasComQuantidadeMaiorQueZero()
    {
        // Arrange
        var contaMaster = new ContaGrafica(null!);
        contaMaster.AtualizarTipo("MASTER");
        await _context.ContasGraficas.AddAsync(contaMaster);
        await _context.SaveChangesAsync();
        
        var custodia1 = new Custodia(contaMaster.Id);
        var custodia2 = new Custodia(contaMaster.Id);
        custodia1.AtualizarCustodia(100, 25m, "PETR4");
        custodia2.AtualizarCustodia(0, 0m, "VALE3"); // Quantidade zero
        
        await _context.Custodias.AddRangeAsync(custodia1, custodia2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterResiduosAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Quantidade.Should().Be(100);
        result.First().Ticker.Should().Be("PETR4");
    }

    [Fact]
    public async Task AddAsync_ContaValida_DeveAdicionarConta()
    {
        // Arrange
        var conta = new ContaGrafica(null!);
        conta.AtualizarTipo("FILHOTE");

        // Act
        await _repository.AddAsync(conta);
        await _repository.SaveChangesAsync(); // Salva as mudanças

        // Assert
        var contas = await _context.ContasGraficas.ToListAsync();
        contas.Should().Contain(conta);
    }

    [Fact]
    public async Task UpdateAsync_ContaValida_DeveAtualizarConta()
    {
        // Arrange
        var conta = new ContaGrafica(null!);
        conta.AtualizarTipo("FILHOTE");
        await _context.ContasGraficas.AddAsync(conta);
        await _context.SaveChangesAsync();

        // Act
        await _repository.UpdateAsync(conta);
        await _repository.SaveChangesAsync();

        // Assert - Verifica se a conta foi atualizada (não lança exceção)
        var contas = await _context.ContasGraficas.ToListAsync();
        contas.Should().Contain(conta);
    }

    [Fact]
    public async Task SaveChangesAsync_DeveChamarSaveChangesDoContexto()
    {
        // Arrange
        var conta = new ContaGrafica(null!);
        conta.AtualizarTipo("FILHOTE");
        await _context.ContasGraficas.AddAsync(conta);

        // Act
        await _repository.SaveChangesAsync();

        // Assert
        // Verifica se a conta foi salva no banco
        var contas = await _context.ContasGraficas.ToListAsync();
        contas.Should().HaveCount(1);
        contas.First().Should().Be(conta);
    }
}
