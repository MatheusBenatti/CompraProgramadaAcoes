using CompraProgramadaAcoes.Infrastructure.Repositories;
using CompraProgramadaAcoes.Infrastructure.Persistence;
using CompraProgramadaAcoes.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore.InMemory;

namespace CompraProgramadaAcoes.UnitTests.Infrastructure.Repositories;

public class DistribuicaoRepositoryTests
{
    private readonly AppDbContext _context;
    private readonly DistribuicaoRepository _repository;

    public DistribuicaoRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _repository = new DistribuicaoRepository(_context);
    }

    [Fact]
    public async Task ObterPorIdAsync_IdValido_DeveRetornarDistribuicaoComRelacionamentos()
    {
        // Arrange
        var cliente = new Cliente("João Silva", "12345678901", "joao@email.com", 1000m);
        await _context.Clientes.AddAsync(cliente);
        await _context.SaveChangesAsync();
        
        var contaGrafica = new ContaGrafica(cliente.Id);
        await _context.ContasGraficas.AddAsync(contaGrafica);
        await _context.SaveChangesAsync();
        
        var custodia = new Custodia(contaGrafica.Id);
        custodia.AtualizarCustodia(100, 25m, "PETR4");
        await _context.Custodias.AddAsync(custodia);
        await _context.SaveChangesAsync();
        
        var ordem = new OrdemCompra(1, "PETR4", 40, 25m, TipoMercado.Lote);
        await _context.OrdensCompra.AddAsync(ordem);
        await _context.SaveChangesAsync();
        
        var distribuicao = new Distribuicao(ordem.Id, custodia.Id, "PETR4", 50, 25m);
        await _context.Distribuicoes.AddAsync(distribuicao);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterPorIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Ticker.Should().Be("PETR4");
        result.OrdemCompra.Should().NotBeNull();
        result.CustodiaFilhote.Should().NotBeNull();
    }

    [Fact]
    public async Task ObterPorIdAsync_IdInvalido_DeveRetornarNulo()
    {
        // Arrange
        var distribuicao = new Distribuicao(1, 1, "PETR4", 50, 25m);
        await _context.Distribuicoes.AddAsync(distribuicao);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterPorIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ObterPorOrdemCompraAsync_OrdemCompraIdValido_DeveRetornarDistribuicoes()
    {
        // Arrange
        var cliente = new Cliente("João Silva", "12345678901", "joao@email.com", 1000m);
        await _context.Clientes.AddAsync(cliente);
        await _context.SaveChangesAsync();
        
        var contaGrafica = new ContaGrafica(cliente.Id);
        await _context.ContasGraficas.AddAsync(contaGrafica);
        await _context.SaveChangesAsync();
        
        var custodia1 = new Custodia(contaGrafica.Id);
        var custodia2 = new Custodia(contaGrafica.Id);
        custodia1.AtualizarCustodia(100, 25m, "PETR4");
        custodia2.AtualizarCustodia(50, 15m, "VALE3");
        await _context.Custodias.AddRangeAsync(custodia1, custodia2);
        await _context.SaveChangesAsync();
        
        var ordem = new OrdemCompra(1, "PETR4", 40, 25m, TipoMercado.Lote);
        await _context.OrdensCompra.AddAsync(ordem);
        await _context.SaveChangesAsync();
        
        var distribuicao1 = new Distribuicao(ordem.Id, custodia1.Id, "PETR4", 50, 25m);
        var distribuicao2 = new Distribuicao(ordem.Id, custodia2.Id, "VALE3", 30, 15m);
        await _context.Distribuicoes.AddRangeAsync(distribuicao1, distribuicao2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterPorOrdemCompraAsync(1);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(d => d.Ticker == "PETR4");
        result.Should().Contain(d => d.Ticker == "VALE3");
    }

    [Fact]
    public async Task ObterPorClienteAsync_ClienteIdValido_DeveRetornarDistribuicoesOrdenadasPorData()
    {
        // Arrange
        var cliente = new Cliente("João Silva", "12345678901", "joao@email.com", 1000m);
        await _context.Clientes.AddAsync(cliente);
        await _context.SaveChangesAsync();
        
        var contaGrafica = new ContaGrafica(cliente.Id);
        await _context.ContasGraficas.AddAsync(contaGrafica);
        await _context.SaveChangesAsync();
        
        var custodia1 = new Custodia(contaGrafica.Id);
        var custodia2 = new Custodia(contaGrafica.Id);
        await _context.Custodias.AddRangeAsync(custodia1, custodia2);
        await _context.SaveChangesAsync();
        
        var ordem1 = new OrdemCompra(1, "PETR4", 40, 25m, TipoMercado.Lote);
        var ordem2 = new OrdemCompra(1, "VALE3", 60, 15m, TipoMercado.Lote);
        await _context.OrdensCompra.AddRangeAsync(ordem1, ordem2);
        await _context.SaveChangesAsync();
        
        var distribuicao1 = new Distribuicao(ordem1.Id, custodia1.Id, "PETR4", 50, 25m);
        var distribuicao2 = new Distribuicao(ordem2.Id, custodia2.Id, "VALE3", 30, 15m);
        
        // Usando o método AtualizarDataDistribuicao
        distribuicao1.AtualizarDataDistribuicao(DateTime.Today.AddDays(-1));
        distribuicao2.AtualizarDataDistribuicao(DateTime.Today);
        
        await _context.Distribuicoes.AddRangeAsync(distribuicao1, distribuicao2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterPorClienteAsync(1);

        // Assert
        result.Should().HaveCount(2);
        result.First().DataDistribuicao.Should().BeCloseTo(DateTime.Today, TimeSpan.FromMinutes(1)); // Ordenado por DataDistribuicao descendente
        result.Last().DataDistribuicao.Should().BeCloseTo(DateTime.Today.AddDays(-1), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task ObterPorPeriodoAsync_PeriodoValido_DeveRetornarDistribuicoesNoPeriodo()
    {
        // Arrange
        var cliente = new Cliente("João Silva", "12345678901", "joao@email.com", 1000m);
        await _context.Clientes.AddAsync(cliente);
        await _context.SaveChangesAsync();
        
        var contaGrafica = new ContaGrafica(cliente.Id);
        await _context.ContasGraficas.AddAsync(contaGrafica);
        await _context.SaveChangesAsync();
        
        var custodia1 = new Custodia(contaGrafica.Id);
        var custodia2 = new Custodia(contaGrafica.Id);
        var custodia3 = new Custodia(contaGrafica.Id);
        await _context.Custodias.AddRangeAsync(custodia1, custodia2, custodia3);
        await _context.SaveChangesAsync();
        
        var ordem1 = new OrdemCompra(1, "PETR4", 40, 25m, TipoMercado.Lote);
        var ordem2 = new OrdemCompra(1, "VALE3", 60, 15m, TipoMercado.Lote);
        var ordem3 = new OrdemCompra(1, "ITUB4", 30, 10m, TipoMercado.Lote);
        await _context.OrdensCompra.AddRangeAsync(ordem1, ordem2, ordem3);
        await _context.SaveChangesAsync();
        
        var distribuicao1 = new Distribuicao(ordem1.Id, custodia1.Id, "PETR4", 50, 25m);
        var distribuicao2 = new Distribuicao(ordem2.Id, custodia2.Id, "VALE3", 30, 15m);
        var distribuicao3 = new Distribuicao(ordem3.Id, custodia3.Id, "ITUB4", 20, 10m);
        
        // Usando o método AtualizarDataDistribuicao
        distribuicao1.AtualizarDataDistribuicao(DateTime.Today.AddDays(-1));
        distribuicao2.AtualizarDataDistribuicao(DateTime.Today);
        distribuicao3.AtualizarDataDistribuicao(DateTime.Today.AddDays(-2));
        
        await _context.Distribuicoes.AddRangeAsync(distribuicao1, distribuicao2, distribuicao3);
        await _context.SaveChangesAsync();

        var inicio = DateTime.Today.AddDays(-1);
        var fim = DateTime.Today;

        // Act
        var result = await _repository.ObterPorPeriodoAsync(inicio, fim);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(d => d.DataDistribuicao.Date == DateTime.Today.Date);
        result.Should().Contain(d => d.DataDistribuicao.Date == DateTime.Today.AddDays(-1).Date);
    }

    [Fact]
    public async Task ObterPorDataReferenciaAsync_DataReferenciaValida_DeveRetornarDistribuicoesDaData()
    {
        // Arrange
        var cliente = new Cliente("João Silva", "12345678901", "joao@email.com", 1000m);
        await _context.Clientes.AddAsync(cliente);
        await _context.SaveChangesAsync();
        
        var contaGrafica = new ContaGrafica(cliente.Id);
        await _context.ContasGraficas.AddAsync(contaGrafica);
        await _context.SaveChangesAsync();
        
        var custodia1 = new Custodia(contaGrafica.Id);
        var custodia2 = new Custodia(contaGrafica.Id);
        var custodia3 = new Custodia(contaGrafica.Id);
        await _context.Custodias.AddRangeAsync(custodia1, custodia2, custodia3);
        await _context.SaveChangesAsync();
        
        var ordem1 = new OrdemCompra(1, "PETR4", 40, 25m, TipoMercado.Lote);
        var ordem2 = new OrdemCompra(1, "VALE3", 60, 15m, TipoMercado.Lote);
        var ordem3 = new OrdemCompra(1, "ITUB4", 30, 10m, TipoMercado.Lote);
        await _context.OrdensCompra.AddRangeAsync(ordem1, ordem2, ordem3);
        await _context.SaveChangesAsync();
        
        var distribuicao1 = new Distribuicao(ordem1.Id, custodia1.Id, "PETR4", 50, 25m);
        var distribuicao2 = new Distribuicao(ordem2.Id, custodia2.Id, "VALE3", 30, 15m);
        var distribuicao3 = new Distribuicao(ordem3.Id, custodia3.Id, "ITUB4", 20, 10m);
        
        // Usando o método AtualizarDataDistribuicao
        distribuicao1.AtualizarDataDistribuicao(DateTime.Today.AddHours(10));
        distribuicao2.AtualizarDataDistribuicao(DateTime.Today.AddHours(15));
        distribuicao3.AtualizarDataDistribuicao(DateTime.Today.AddDays(-1));
        
        await _context.Distribuicoes.AddRangeAsync(distribuicao1, distribuicao2, distribuicao3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterPorDataReferenciaAsync(DateTime.Today);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(d => d.DataDistribuicao.Date == DateTime.Today.Date);
    }

    [Fact]
    public async Task AddAsync_DistribuicaoValida_DeveAdicionarDistribuicao()
    {
        // Arrange
        var distribuicao = new Distribuicao(1, 1, "PETR4", 50, 25m);

        // Act
        await _repository.AddAsync(distribuicao);
        await _repository.SaveChangesAsync(); // Salva as mudanças

        // Assert
        // Verifica se foi adicionado ao contexto
        var distribuicoes = await _context.Distribuicoes.ToListAsync();
        distribuicoes.Should().Contain(distribuicao);
    }

    [Fact]
    public async Task UpdateAsync_DistribuicaoValida_DeveAtualizarDistribuicao()
    {
        // Arrange
        var distribuicao = new Distribuicao(1, 1, "PETR4", 50, 25m);
        await _context.Distribuicoes.AddAsync(distribuicao);
        await _context.SaveChangesAsync();

        // Act
        await _repository.UpdateAsync(distribuicao);
        await _repository.SaveChangesAsync();

        // Assert - Verifica se a distribuição foi atualizada (não lança exceção)
        var distribuicoes = await _context.Distribuicoes.ToListAsync();
        distribuicoes.Should().Contain(distribuicao);
    }

    [Fact]
    public async Task SaveChangesAsync_DeveChamarSaveChangesDoContexto()
    {
        // Arrange
        var distribuicao = new Distribuicao(1, 1, "PETR4", 50, 25m);
        await _context.Distribuicoes.AddAsync(distribuicao);

        // Act
        await _repository.SaveChangesAsync();

        // Assert
        // Verifica se a distribuição foi salva no banco
        var distribuicoes = await _context.Distribuicoes.ToListAsync();
        distribuicoes.Should().HaveCount(1);
        distribuicoes.First().Should().Be(distribuicao);
    }
}
