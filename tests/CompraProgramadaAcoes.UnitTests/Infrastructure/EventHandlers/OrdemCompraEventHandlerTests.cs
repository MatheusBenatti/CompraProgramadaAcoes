using CompraProgramadaAcoes.Infrastructure.EventHandlers;
using CompraProgramadaAcoes.Domain.Events;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using CompraProgramadaAcoes.Domain.ValueObjects;

namespace CompraProgramadaAcoes.UnitTests.Infrastructure.EventHandlers;

public class OrdemCompraEventHandlerTests
{
    private readonly Mock<IOrdemCompraRepository> _ordemCompraRepositoryMock;
    private readonly Mock<IMessagePublisher> _messagePublisherMock;
    private readonly Mock<ILogger<OrdemCompraEventHandler>> _loggerMock;
    private readonly OrdemCompraEventHandler _handler;

    public OrdemCompraEventHandlerTests()
    {
        _ordemCompraRepositoryMock = new Mock<IOrdemCompraRepository>();
        _messagePublisherMock = new Mock<IMessagePublisher>();
        _loggerMock = new Mock<ILogger<OrdemCompraEventHandler>>();
        
        _handler = new OrdemCompraEventHandler(
            _ordemCompraRepositoryMock.Object,
            _messagePublisherMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_CompraConsolidadaEventValido_DeveProcessarEPublicarEvento()
    {
        // Arrange
        var contaMasterId = Guid.NewGuid();
        var comprasRealizadas = new Dictionary<Ticker, (int Quantidade, decimal ValorTotal)>
        {
            { new Ticker("PETR4"), (100, 5000m) },
            { new Ticker("VALE3"), (50, 2500m) }
        };
        var valorTotal = new ValorMonetario(7500m);
        var dataCompra = DateTime.UtcNow;

        var @event = new CompraConsolidadaEvent(contaMasterId, comprasRealizadas, valorTotal, dataCompra);

        // Act
        await _handler.Handle(@event);

        // Assert
        VerifyLoggerLoggedInformation($"Processando compra consolidada para a conta master {contaMasterId}");
        VerifyLoggerLoggedInformation($"Evento CompraConsolidada publicado no Kafka para a conta {contaMasterId}");

        _messagePublisherMock.Verify(p => p.PublishAsync(
            "compra-events",
            It.Is<string>(json => json.Contains("\"Evento\":\"CompraConsolidada\"") && 
                                  json.Contains($"\"ContaMasterId\":\"{contaMasterId}\"") &&
                                  json.Contains("\"Ticker\":\"PETR4\"") &&
                                  json.Contains("\"Quantidade\":100") &&
                                  json.Contains("\"ValorTotal\":5000") &&
                                  json.Contains("\"Ticker\":\"VALE3\"") &&
                                  json.Contains("\"Quantidade\":50") &&
                                  json.Contains("\"ValorTotal\":2500") &&
                                  json.Contains("\"ValorTotalConsolidado\":7500") &&
                                  json.Contains("\"Status\":\"Executada\"") &&
                                  json.Contains("\"Origem\":\"CompraProgramadaAcoes\""))));
    }

    [Fact]
    public async Task Handle_InvestimentoRealizadoEventValido_DeveProcessarEPublicarEvento()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var valorInvestido = new ValorMonetario(10000m);
        var ativosDistribuidos = new Dictionary<Ticker, int>
        {
            { new Ticker("PETR4"), 100 },
            { new Ticker("VALE3"), 50 }
        };
        var dataInvestimento = DateTime.UtcNow;

        var @event = new InvestimentoRealizadoEvent(clienteId, valorInvestido, ativosDistribuidos, dataInvestimento);

        // Act
        await _handler.Handle(@event);

        // Assert
        VerifyLoggerLoggedInformation($"Processando investimento realizado para o cliente {clienteId}");
        VerifyLoggerLoggedInformation($"Evento InvestimentoRealizado publicado no Kafka para o cliente {clienteId}");

        _messagePublisherMock.Verify(p => p.PublishAsync(
            "investimento-events",
            It.Is<string>(json => json.Contains("\"Evento\":\"InvestimentoRealizado\"") && 
                                  json.Contains($"\"ClienteId\":\"{clienteId}\"") &&
                                  json.Contains("\"ValorInvestido\":10000") &&
                                  json.Contains("\"Ticker\":\"PETR4\"") &&
                                  json.Contains("\"Quantidade\":100") &&
                                  json.Contains("\"Ticker\":\"VALE3\"") &&
                                  json.Contains("\"Quantidade\":50") &&
                                  json.Contains("\"Origem\":\"CompraProgramadaAcoes\""))));
    }

    private void VerifyLoggerLoggedInformation(string expectedMessage)
    {
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
