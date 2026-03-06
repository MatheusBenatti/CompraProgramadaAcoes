using CompraProgramadaAcoes.Infrastructure.Repositories;
using CompraProgramadaAcoes.Infrastructure.Persistence;
using CompraProgramadaAcoes.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore.InMemory;

namespace CompraProgramadaAcoes.UnitTests.Infrastructure.Repositories;

public class HistoricoValorMensalRepositoryTests
{
    private readonly AppDbContext _context;
    private readonly HistoricoValorMensalRepository _repository;

    public HistoricoValorMensalRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _repository = new HistoricoValorMensalRepository(_context);
    }

    [Fact]
    public async Task ObterPorClienteIdAsync_ClienteIdValido_DeveRetornarHistoricoMaisRecente()
    {
        // Arrange
        // Como não temos método para atualizar data, vamos criar instâncias com datas diferentes
        var historico1 = new HistoricoValorMensal(1, 800m, 1000m);
        var historico2 = new HistoricoValorMensal(1, 1000m, 1500m);
        var historico3 = new HistoricoValorMensal(1, 1500m, 2000m);
        
        // Usando reflection para setar as datas (apenas para teste)
        typeof(HistoricoValorMensal).GetProperty(nameof(HistoricoValorMensal.DataAlteracao))!
            .SetValue(historico1, DateTime.Today.AddDays(-2));
        typeof(HistoricoValorMensal).GetProperty(nameof(HistoricoValorMensal.DataAlteracao))!
            .SetValue(historico2, DateTime.Today.AddDays(-1));
        typeof(HistoricoValorMensal).GetProperty(nameof(HistoricoValorMensal.DataAlteracao))!
            .SetValue(historico3, DateTime.Today);

        await _context.HistoricoValoresMensais.AddRangeAsync(historico1, historico2, historico3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterPorClienteIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.ValorNovo.Should().Be(2000m); // Mais recente
        result.DataAlteracao.Should().BeCloseTo(DateTime.Today, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task ObterPorClienteIdAsync_ClienteIdInvalido_DeveRetornarNulo()
    {
        // Arrange
        var historico1 = new HistoricoValorMensal(1, 800m, 1000m);
        var historico2 = new HistoricoValorMensal(2, 1000m, 1500m);

        await _context.HistoricoValoresMensais.AddRangeAsync(historico1, historico2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterPorClienteIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ObterHistoricoAsync_ClienteIdValido_DeveRetornarHistoricoOrdenadoPorData()
    {
        // Arrange
        // Como não temos método para atualizar data, vamos criar instâncias com datas diferentes
        var historico1 = new HistoricoValorMensal(1, 800m, 1000m);
        var historico2 = new HistoricoValorMensal(1, 1000m, 1500m);
        var historico3 = new HistoricoValorMensal(1, 1500m, 2000m);
        
        // Usando reflection para setar as datas (apenas para teste)
        typeof(HistoricoValorMensal).GetProperty(nameof(HistoricoValorMensal.DataAlteracao))!
            .SetValue(historico1, DateTime.Today.AddDays(-2));
        typeof(HistoricoValorMensal).GetProperty(nameof(HistoricoValorMensal.DataAlteracao))!
            .SetValue(historico2, DateTime.Today.AddDays(-1));
        typeof(HistoricoValorMensal).GetProperty(nameof(HistoricoValorMensal.DataAlteracao))!
            .SetValue(historico3, DateTime.Today);

        await _context.HistoricoValoresMensais.AddRangeAsync(historico1, historico2, historico3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterHistoricoAsync(1);

        // Assert
        result.Should().HaveCount(3);
        result.First().DataAlteracao.Should().BeCloseTo(DateTime.Today, TimeSpan.FromMinutes(1)); // Ordenado por DataAlteracao descendente
        result.Last().DataAlteracao.Should().BeCloseTo(DateTime.Today.AddDays(-2), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task ObterHistoricoAsync_ClienteSemHistorico_DeveRetornarListaVazia()
    {
        // Arrange
        var historico1 = new HistoricoValorMensal(1, 800m, 1000m);
        var historico2 = new HistoricoValorMensal(2, 1000m, 1500m);

        await _context.HistoricoValoresMensais.AddRangeAsync(historico1, historico2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterHistoricoAsync(999);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAsync_HistoricoValido_DeveAdicionarHistorico()
    {
        // Arrange
        var historico = new HistoricoValorMensal(1, 800m, 1000m);

        // Act
        var result = await _repository.AddAsync(historico);
        await _repository.SaveChangesAsync(); // Salva as mudanças

        // Assert
        result.Should().Be(historico);
        
        // Verifica se foi adicionado ao contexto
        var historicos = await _context.HistoricoValoresMensais.ToListAsync();
        historicos.Should().Contain(historico);
    }

    [Fact]
    public async Task SaveChangesAsync_DeveChamarSaveChangesDoContexto()
    {
        // Arrange
        var historico = new HistoricoValorMensal(1, 800m, 1000m);
        await _context.HistoricoValoresMensais.AddAsync(historico);

        // Act
        var result = await _repository.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        
        // Verifica se o histórico foi salvo no banco
        var historicos = await _context.HistoricoValoresMensais.ToListAsync();
        historicos.Should().HaveCount(1);
        historicos.First().Should().Be(historico);
    }

    [Theory]
    [InlineData(1000.50)]
    [InlineData(2500.75)]
    [InlineData(5000.00)]
    public async Task AddAsync_ValoresDiferentes_DeveAdicionarHistoricoComValorCorreto(decimal valor)
    {
        // Arrange
        var historico = new HistoricoValorMensal(1, 800m, valor);

        // Act
        var result = await _repository.AddAsync(historico);

        // Assert
        result.Should().Be(historico);
        result.ValorNovo.Should().Be(valor);
        
        // Verifica se foi adicionado ao contexto
        await _context.SaveChangesAsync();
        var historicos = await _context.HistoricoValoresMensais.ToListAsync();
        historicos.Should().Contain(h => h.ValorNovo == valor);
    }

    [Fact]
    public async Task ObterPorClienteIdAsync_MultiplosHistoricos_DeveRetornarOMaisRecente()
    {
        // Arrange
        // Como não temos método para atualizar data, vamos criar instâncias com datas diferentes
        var historicoAntigo = new HistoricoValorMensal(1, 300m, 500m);
        var historicoMedio = new HistoricoValorMensal(1, 500m, 1000m);
        var historicoRecente = new HistoricoValorMensal(1, 1000m, 1500m);
        
        // Usando reflection para setar as datas (apenas para teste)
        typeof(HistoricoValorMensal).GetProperty(nameof(HistoricoValorMensal.DataAlteracao))!
            .SetValue(historicoAntigo, DateTime.Today.AddDays(-2));
        typeof(HistoricoValorMensal).GetProperty(nameof(HistoricoValorMensal.DataAlteracao))!
            .SetValue(historicoMedio, DateTime.Today.AddDays(-1));
        typeof(HistoricoValorMensal).GetProperty(nameof(HistoricoValorMensal.DataAlteracao))!
            .SetValue(historicoRecente, DateTime.Today);

        await _context.HistoricoValoresMensais.AddRangeAsync(historicoAntigo, historicoMedio, historicoRecente);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterPorClienteIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.ValorNovo.Should().Be(1500m);
        result.DataAlteracao.Should().BeCloseTo(DateTime.Today, TimeSpan.FromMinutes(1));
    }
}
