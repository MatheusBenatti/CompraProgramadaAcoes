using CompraProgramadaAcoes.Infrastructure.EventHandlers;
using CompraProgramadaAcoes.Domain.Events;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using CompraProgramadaAcoes.Domain.ValueObjects;

namespace CompraProgramadaAcoes.UnitTests.Infrastructure.EventHandlers;

public class ContaEventHandlerTests
{
    private readonly Mock<IContaGraficaRepository> _contaGraficaRepositoryMock;
    private readonly Mock<IMessagePublisher> _messagePublisherMock;
    private readonly Mock<ILogger<ContaEventHandler>> _loggerMock;
    private readonly ContaEventHandler _handler;

    public ContaEventHandlerTests()
    {
        _contaGraficaRepositoryMock = new Mock<IContaGraficaRepository>();
        _messagePublisherMock = new Mock<IMessagePublisher>();
        _loggerMock = new Mock<ILogger<ContaEventHandler>>();
        
        _handler = new ContaEventHandler(
            _contaGraficaRepositoryMock.Object,
            _messagePublisherMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_AtivosDistribuidosEventValido_DeveProcessarEPublicarEvento()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var ativosRecebidos = new Dictionary<Ticker, int>
        {
            { new Ticker("PETR4"), 100 },
            { new Ticker("VALE3"), 50 }
        };
        var valorTotal = new ValorMonetario(10000m);
        var dataDistribuicao = DateTime.UtcNow;

        var @event = new AtivosDistribuidosEvent(clienteId, ativosRecebidos, valorTotal, dataDistribuicao);

        // Act
        await _handler.Handle(@event);

        // Assert
        VerifyLoggerLoggedInformation($"Processando distribuição de ativos para o cliente {clienteId}");
        VerifyLoggerLoggedInformation($"Publicando evento de distribuição para cliente {clienteId} (busca de conta desabilitada devido à incompatibilidade de tipos)");
        VerifyLoggerLoggedInformation($"Evento AtivosDistribuidos publicado no Kafka para o cliente {clienteId}");

        _messagePublisherMock.Verify(p => p.PublishAsync(
            "distribuicao-events",
            It.Is<string>(json => json.Contains("\"Evento\":\"AtivosDistribuidos\"") && 
                                  json.Contains($"\"ClienteId\":\"{clienteId}\"") &&
                                  json.Contains("\"PETR4\":100") &&
                                  json.Contains("\"VALE3\":50") &&
                                  json.Contains("\"ValorTotalDistribuido\":10000") &&
                                  json.Contains("\"Origem\":\"CompraProgramadaAcoes\""))));
    }

    [Fact]
    public async Task Handle_AtivosDistribuidosEventComAtivosVazios_DeveProcessarEPublicarEvento()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var ativosRecebidos = new Dictionary<Ticker, int>();
        var valorTotal = new ValorMonetario(0m);
        var dataDistribuicao = DateTime.UtcNow;

        var @event = new AtivosDistribuidosEvent(clienteId, ativosRecebidos, valorTotal, dataDistribuicao);

        // Act
        await _handler.Handle(@event);

        // Assert
        VerifyLoggerLoggedInformation($"Processando distribuição de ativos para o cliente {clienteId}");
        VerifyLoggerLoggedInformation($"Publicando evento de distribuição para cliente {clienteId} (busca de conta desabilitada devido à incompatibilidade de tipos)");
        VerifyLoggerLoggedInformation($"Evento AtivosDistribuidos publicado no Kafka para o cliente {clienteId}");

        _messagePublisherMock.Verify(p => p.PublishAsync(
            "distribuicao-events",
            It.Is<string>(json => json.Contains("\"Evento\":\"AtivosDistribuidos\"") && 
                                  json.Contains($"\"ClienteId\":\"{clienteId}\"") &&
                                  json.Contains("\"AtivosRecebidos\":{}") &&
                                  json.Contains("\"ValorTotalDistribuido\":0") &&
                                  json.Contains("\"Origem\":\"CompraProgramadaAcoes\""))));
    }

    [Fact]
    public async Task Handle_AtivosDistribuidosEventComMultiplosAtivos_DeveProcessarEPublicarEventoCorretamente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var ativosRecebidos = new Dictionary<Ticker, int>
        {
            { new Ticker("PETR4"), 100 },
            { new Ticker("VALE3"), 50 },
            { new Ticker("ITUB4"), 75 },
            { new Ticker("BBDC4"), 25 }
        };
        var valorTotal = new ValorMonetario(25000m);
        var dataDistribuicao = DateTime.UtcNow;

        var @event = new AtivosDistribuidosEvent(clienteId, ativosRecebidos, valorTotal, dataDistribuicao);

        // Act
        await _handler.Handle(@event);

        // Assert
        _messagePublisherMock.Verify(p => p.PublishAsync(
            "distribuicao-events",
            It.Is<string>(json => json.Contains("\"PETR4\":100") &&
                                  json.Contains("\"VALE3\":50") &&
                                  json.Contains("\"ITUB4\":75") &&
                                  json.Contains("\"BBDC4\":25") &&
                                  json.Contains("\"ValorTotalDistribuido\":25000"))));
    }

    [Fact]
    public async Task Handle_MessagePublisherLancaExcecao_DeveLogarErroEPropagarExcecao()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var ativosRecebidos = new Dictionary<Ticker, int>
        {
            { new Ticker("PETR4"), 100 }
        };
        var valorTotal = new ValorMonetario(5000m);
        var dataDistribuicao = DateTime.UtcNow;

        var @event = new AtivosDistribuidosEvent(clienteId, ativosRecebidos, valorTotal, dataDistribuicao);
        var expectedException = new InvalidOperationException("Kafka connection failed");

        _messagePublisherMock.Setup(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(@event));
        exception.Should().Be(expectedException);

        VerifyLoggerLoggedInformation($"Processando distribuição de ativos para o cliente {clienteId}");
        VerifyLoggerLoggedInformation($"Publicando evento de distribuição para cliente {clienteId} (busca de conta desabilitada devido à incompatibilidade de tipos)");
    }

    [Theory]
    [InlineData(1000.50)]
    [InlineData(5000.75)]
    [InlineData(10000.00)]
    public async Task Handle_DiferentesValoresTotais_DevePublicarEventoComValorCorreto(decimal valorTotal)
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var ativosRecebidos = new Dictionary<Ticker, int>
        {
            { new Ticker("PETR4"), 100 }
        };
        var valorMonetario = new ValorMonetario(valorTotal);
        var dataDistribuicao = DateTime.UtcNow;

        var @event = new AtivosDistribuidosEvent(clienteId, ativosRecebidos, valorMonetario, dataDistribuicao);

        // Act
        await _handler.Handle(@event);

        // Assert
        _messagePublisherMock.Verify(p => p.PublishAsync(
            "distribuicao-events",
            It.Is<string>(json => json.Contains($"\"ValorTotalDistribuido\":{valorTotal}"))));
    }

    [Fact]
    public async Task Handle_DataDistribuicaoEspecifica_DevePublicarEventoComDataCorreta()
    {
        // Arrange
        var clienteId = Guid.NewGuid();
        var ativosRecebidos = new Dictionary<Ticker, int>
        {
            { new Ticker("PETR4"), 100 }
        };
        var valorTotal = new ValorMonetario(5000m);
        var dataDistribuicao = new DateTime(2024, 3, 15, 14, 30, 0);

        var @event = new AtivosDistribuidosEvent(clienteId, ativosRecebidos, valorTotal, dataDistribuicao);

        // Act
        await _handler.Handle(@event);

        // Assert
        _messagePublisherMock.Verify(p => p.PublishAsync(
            "distribuicao-events",
            It.Is<string>(json => json.Contains("\"DataDistribuicao\":\"2024-03-15T14:30:00\""))));
    }

    [Fact]
    public async Task Handle_MultiplasChamadas_DevePublicarEventosIndependentes()
    {
        // Arrange
        var clienteId1 = Guid.NewGuid();
        var clienteId2 = Guid.NewGuid();
        var ativosRecebidos = new Dictionary<Ticker, int>
        {
            { new Ticker("PETR4"), 100 }
        };
        var valorTotal = new ValorMonetario(5000m);
        var dataDistribuicao = DateTime.UtcNow;

        var event1 = new AtivosDistribuidosEvent(clienteId1, ativosRecebidos, valorTotal, dataDistribuicao);
        var event2 = new AtivosDistribuidosEvent(clienteId2, ativosRecebidos, valorTotal, dataDistribuicao);

        // Act
        await _handler.Handle(event1);
        await _handler.Handle(event2);

        // Assert
        _messagePublisherMock.Verify(p => p.PublishAsync(
            "distribuicao-events",
            It.Is<string>(json => json.Contains($"\"ClienteId\":\"{clienteId1}\""))));

        _messagePublisherMock.Verify(p => p.PublishAsync(
            "distribuicao-events",
            It.Is<string>(json => json.Contains($"\"ClienteId\":\"{clienteId2}\""))));

        _messagePublisherMock.Verify(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
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
