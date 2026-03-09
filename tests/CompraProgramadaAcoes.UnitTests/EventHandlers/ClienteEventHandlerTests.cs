using CompraProgramadaAcoes.Infrastructure.EventHandlers;
using CompraProgramadaAcoes.Domain.Events;
using CompraProgramadaAcoes.Domain.ValueObjects;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace CompraProgramadaAcoes.UnitTests.EventHandlers;

public class ClienteEventHandlerTests
{
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<IMessagePublisher> _messagePublisherMock;
    private readonly Mock<ILogger<ClienteEventHandler>> _loggerMock;
    private readonly ClienteEventHandler _eventHandler;

    public ClienteEventHandlerTests()
    {
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _messagePublisherMock = new Mock<IMessagePublisher>();
        _loggerMock = new Mock<ILogger<ClienteEventHandler>>();
        _eventHandler = new ClienteEventHandler(_clienteRepositoryMock.Object, _messagePublisherMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ClienteCriadoEvent_DeveLogarMensagemCorreta()
    {
        // Arrange
        var clienteId = 123L;
        var @event = new ClienteCriadoEvent(
            clienteId,
            "Teste",
            new CPF("12345678901"),
            new Email("teste@email.com"),
            new ValorMonetario(500),
            DateTime.UtcNow
        );

        // Act
        await _eventHandler.Handle(@event);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Processando criação do cliente {clienteId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ClienteDesativadoEvent_DeveLogarMensagemCorreta()
    {
        // Arrange
        var clienteId = 123L;
        var motivo = "Solicitacao do cliente";
        var @event = new ClienteDesativadoEvent(
            clienteId,
            motivo,
            DateTime.UtcNow
        );

        // Act
        await _eventHandler.Handle(@event);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Processando desativação do cliente {clienteId}: {motivo}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValorMensalAlteradoEvent_DeveLogarMensagemCorreta()
    {
        // Arrange
        var clienteId = 123L;
        var valorAnterior = new ValorMonetario(500);
        var valorNovo = new ValorMonetario(1000);
        var @event = new ValorMensalAlteradoEvent(
            clienteId,
            valorAnterior,
            valorNovo,
            DateTime.UtcNow
        );

        // Act
        await _eventHandler.Handle(@event);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Processando alteração de valor mensal do cliente {clienteId}: {valorAnterior} -> {valorNovo}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ClienteCriadoEvent_DeveRetornarTaskCompleted()
    {
        // Arrange
        var @event = new ClienteCriadoEvent(
            123L,
            "Teste",
            new CPF("12345678901"),
            new Email("teste@email.com"),
            new ValorMonetario(500),
            DateTime.UtcNow
        );

        // Act
        await _eventHandler.Handle(@event);
        _clienteRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ClienteDesativadoEvent_DeveRetornarTaskCompleted()
    {
        // Arrange
        var @event = new ClienteDesativadoEvent(
            123L,
            "Teste",
            DateTime.UtcNow
        );

        // Act
        await _eventHandler.Handle(@event);
        _clienteRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_ValorMensalAlteradoEvent_DeveRetornarTaskCompleted()
    {
        // Arrange
        var @event = new ValorMensalAlteradoEvent(
            123L,
            new ValorMonetario(500),
            new ValorMonetario(1000),
            DateTime.UtcNow
        );

        // Act
        await _eventHandler.Handle(@event);
        _clienteRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }
}
