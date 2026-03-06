using CompraProgramadaAcoes.Infrastructure.Repositories;
using CompraProgramadaAcoes.Infrastructure.Persistence;
using CompraProgramadaAcoes.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Moq;
using Microsoft.EntityFrameworkCore.InMemory;

namespace CompraProgramadaAcoes.UnitTests.Infrastructure.Repositories;

public class EventoIRRepositoryTests
{
    private readonly AppDbContext _context;
    private readonly EventoIRRepository _repository;

    public EventoIRRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new AppDbContext(options);
        _repository = new EventoIRRepository(_context);
    }

    [Fact]
    public async Task ObterPorIdAsync_IdValido_DeveRetornarEventoComCliente()
    {
        // Arrange
        var cliente = new Cliente("João Silva", "12345678901", "joao@email.com", 1000m);
        await _context.Clientes.AddAsync(cliente);
        await _context.SaveChangesAsync();
        
        var evento = new EventoIR(1, TipoEventoIR.DedoDuro, 1000m, 150m);
        await _context.EventosIR.AddAsync(evento);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterPorIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Tipo.Should().Be(TipoEventoIR.DedoDuro);
        result.ValorBase.Should().Be(1000m);
        result.ValorIR.Should().Be(150m);
        result.Cliente.Should().NotBeNull();
    }

    [Fact]
    public async Task ObterPorIdAsync_IdInvalido_DeveRetornarNulo()
    {
        // Arrange
        var evento = new EventoIR(1, TipoEventoIR.DedoDuro, 1000m, 150m);
        await _context.EventosIR.AddAsync(evento);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterPorIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ObterPorClienteAsync_ClienteIdValido_DeveRetornarEventosOrdenadosPorData()
    {
        // Arrange
        var evento1 = new EventoIR(1, TipoEventoIR.DedoDuro, 1000m, 150m);
        var evento2 = new EventoIR(1, TipoEventoIR.DedoDuro, 2000m, 300m);
        
        // Usando reflection para setar datas diferentes
        typeof(EventoIR).GetProperty(nameof(EventoIR.DataEvento))!
            .SetValue(evento1, DateTime.Today.AddDays(-1));
        typeof(EventoIR).GetProperty(nameof(EventoIR.DataEvento))!
            .SetValue(evento2, DateTime.Today);

        await _context.EventosIR.AddRangeAsync(evento1, evento2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterPorClienteAsync(1);

        // Assert
        result.Should().HaveCount(2);
        result.First().DataEvento.Should().BeCloseTo(DateTime.Today, TimeSpan.FromMinutes(1)); // Ordenado por DataEvento descendente
        result.Last().DataEvento.Should().BeCloseTo(DateTime.Today.AddDays(-1), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task ObterNaoPublicadosAsync_DeveRetornarEventosNaoPublicadosOrdenadosPorData()
    {
        // Arrange
        var evento1 = new EventoIR(1, TipoEventoIR.DedoDuro, 1000m, 150m);
        var evento2 = new EventoIR(1, TipoEventoIR.DedoDuro, 2000m, 300m);
        
        // evento2 não publicado
        evento2.MarcarComoPublicado();
        
        // Usando reflection para setar datas diferentes
        typeof(EventoIR).GetProperty(nameof(EventoIR.DataEvento))!
            .SetValue(evento1, DateTime.Today.AddDays(-1));
        typeof(EventoIR).GetProperty(nameof(EventoIR.DataEvento))!
            .SetValue(evento2, DateTime.Today);

        await _context.EventosIR.AddRangeAsync(evento1, evento2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterNaoPublicadosAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().PublicadoKafka.Should().BeFalse();
        result.First().Tipo.Should().Be(TipoEventoIR.DedoDuro);
    }

    [Fact]
    public async Task ObterPorDataReferenciaAsync_DataReferenciaValida_DeveRetornarEventosDaData()
    {
        // Arrange
        var cliente = new Cliente("João Silva", "12345678901", "joao@email.com", 1000m);
        await _context.Clientes.AddAsync(cliente);
        await _context.SaveChangesAsync();
        
        var dataReferencia = DateTime.Today;
        var evento1 = new EventoIR(cliente.Id, TipoEventoIR.DedoDuro, 1000m, 150m);
        var evento2 = new EventoIR(cliente.Id, TipoEventoIR.DedoDuro, 2000m, 300m);
        var evento3 = new EventoIR(cliente.Id, TipoEventoIR.DedoDuro, 1500m, 225m);
        
        // Usando reflection para setar datas exatas
        typeof(EventoIR).GetProperty(nameof(EventoIR.DataEvento))!
            .SetValue(evento1, dataReferencia.AddHours(10)); // Hoje às 10h
        typeof(EventoIR).GetProperty(nameof(EventoIR.DataEvento))!
            .SetValue(evento2, dataReferencia.AddHours(15)); // Hoje às 15h
        typeof(EventoIR).GetProperty(nameof(EventoIR.DataEvento))!
            .SetValue(evento3, dataReferencia.AddDays(-1).AddHours(12)); // Ontem às 12h

        await _context.EventosIR.AddRangeAsync(evento1, evento2, evento3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ObterPorDataReferenciaAsync(dataReferencia);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.DataEvento.Date == dataReferencia.Date);
        result.Should().BeInDescendingOrder(e => e.DataEvento);
    }

    [Fact]
    public async Task AddAsync_EventoValido_DeveAdicionarEvento()
    {
        // Arrange
        var evento = new EventoIR(1, TipoEventoIR.DedoDuro, 1000m, 150m);

        // Act
        var result = await _repository.AddAsync(evento);
        await _repository.SaveChangesAsync(); // Salva as mudanças

        // Assert
        result.Should().Be(evento);
        
        // Verifica se foi adicionado ao contexto
        var eventos = await _context.EventosIR.ToListAsync();
        eventos.Should().Contain(evento);
    }

    [Fact]
    public async Task UpdateAsync_EventoValido_DeveAtualizarEvento()
    {
        // Arrange
        var evento = new EventoIR(1, TipoEventoIR.DedoDuro, 1000m, 150m);
        await _context.EventosIR.AddAsync(evento);
        await _context.SaveChangesAsync();

        // Act
        await _repository.UpdateAsync(evento);
        await _repository.SaveChangesAsync();

        // Assert - Verifica se o evento foi atualizado (não lança exceção)
        var eventos = await _context.EventosIR.ToListAsync();
        eventos.Should().Contain(evento);
    }

    [Fact]
    public async Task SaveChangesAsync_DeveChamarSaveChangesDoContexto()
    {
        // Arrange
        var evento = new EventoIR(1, TipoEventoIR.DedoDuro, 1000m, 150m);
        await _context.EventosIR.AddAsync(evento);

        // Act
        var result = await _repository.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        
        // Verifica se o evento foi salvo no banco
        var eventos = await _context.EventosIR.ToListAsync();
        eventos.Should().HaveCount(1);
        eventos.First().Should().Be(evento);
    }
}
