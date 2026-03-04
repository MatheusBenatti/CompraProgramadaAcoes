using CompraProgramadaAcoes.Application.Interfaces;
using CompraProgramadaAcoes.Workers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace CompraProgramadaAcoes.UnitTests.Workers;

public class TestableRebalanceamentoWorker : RebalanceamentoWorker
{
    public TestableRebalanceamentoWorker(
        ILogger<RebalanceamentoWorker> logger,
        IServiceProvider serviceProvider) : base(logger, serviceProvider)
    {
    }

    public new Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return base.ExecuteAsync(stoppingToken);
    }
}

public class RebalanceamentoWorkerTests
{
    private readonly Mock<ILogger<RebalanceamentoWorker>> _loggerMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IMotorRebalanceamento> _motorRebalanceamentoMock;
    private readonly TestableRebalanceamentoWorker _worker;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public RebalanceamentoWorkerTests()
    {
        _loggerMock = new Mock<ILogger<RebalanceamentoWorker>>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _motorRebalanceamentoMock = new Mock<IMotorRebalanceamento>();
        _cancellationTokenSource = new CancellationTokenSource();

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(_serviceScopeFactoryMock.Object);

        _serviceScopeFactoryMock
            .Setup(x => x.CreateScope())
            .Returns(_serviceScopeMock.Object);

        _serviceScopeMock
            .Setup(x => x.ServiceProvider)
            .Returns(_serviceProviderMock.Object);

        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IMotorRebalanceamento)))
            .Returns(_motorRebalanceamentoMock.Object);

        _worker = new TestableRebalanceamentoWorker(_loggerMock.Object, _serviceProviderMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DeveExecutarRebalanceamentoPorDesvioProporcao()
    {
        // Arrange
        _motorRebalanceamentoMock
            .Setup(x => x.RebalancearPorDesvioProporcaoAsync(0.10m))
            .Returns(Task.CompletedTask);

        // Act
        var task = _worker.ExecuteAsync(_cancellationTokenSource.Token);
        
        // Aguarda um pouco para a execução iniciar
        await Task.Delay(100);
        
        // Cancela para parar o worker
        _cancellationTokenSource.Cancel();
        
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            // Esperado quando o worker é cancelado
        }

        // Assert
        _motorRebalanceamentoMock.Verify(
            x => x.RebalancearPorDesvioProporcaoAsync(0.10m),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ComExcecao_DeveLogarErroEContinuar()
    {
        // Arrange
        _motorRebalanceamentoMock
            .Setup(x => x.RebalancearPorDesvioProporcaoAsync(It.IsAny<decimal>()))
            .ThrowsAsync(new Exception("Erro de rebalanceamento"));

        // Act
        var task = _worker.ExecuteAsync(_cancellationTokenSource.Token);
        
        // Aguarda um pouco para a execução iniciar
        await Task.Delay(200);
        
        // Cancela para parar o worker
        _cancellationTokenSource.Cancel();
        
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            // Esperado quando o worker é cancelado
        }

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro durante execução do worker de rebalanceamento")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_DeveLogarInicioCorretamente()
    {
        // Arrange
        _motorRebalanceamentoMock
            .Setup(x => x.RebalancearPorDesvioProporcaoAsync(It.IsAny<decimal>()))
            .Returns(Task.CompletedTask);

        // Act
        var task = _worker.ExecuteAsync(_cancellationTokenSource.Token);
        
        // Aguarda um pouco para a execução iniciar
        await Task.Delay(100);
        
        // Cancela para parar o worker
        _cancellationTokenSource.Cancel();
        
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            // Esperado quando o worker é cancelado
        }

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Worker de Rebalanceamento iniciado - Verificação semanal de desvios")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DeveLogarVerificacaoDesvios()
    {
        // Arrange
        _motorRebalanceamentoMock
            .Setup(x => x.RebalancearPorDesvioProporcaoAsync(It.IsAny<decimal>()))
            .Returns(Task.CompletedTask);

        // Act
        var task = _worker.ExecuteAsync(_cancellationTokenSource.Token);
        
        // Aguarda um pouco para a execução iniciar
        await Task.Delay(100);
        
        // Cancela para parar o worker
        _cancellationTokenSource.Cancel();
        
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            // Esperado quando o worker é cancelado
        }

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Verificando rebalanceamento por desvio de proporção")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando verificação de desvios de proporção")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Verificação de desvios de proporção concluída")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task StopAsync_DeveLogarMensagemCorreta()
    {
        // Arrange
        var cancellationToken = new CancellationToken();

        // Act
        await _worker.StopAsync(cancellationToken);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Parando worker de rebalanceamento")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ComMultiplosCiclos_DeveExecutarCorretamente()
    {
        // Arrange
        var execucoes = 0;
        _motorRebalanceamentoMock
            .Setup(x => x.RebalancearPorDesvioProporcaoAsync(0.10m))
            .Returns(() =>
            {
                execucoes++;
                return Task.CompletedTask;
            });

        // Act
        var task = _worker.ExecuteAsync(_cancellationTokenSource.Token);
        
        // Aguarda um pouco para múltiplas execuções (simuladas)
        await Task.Delay(300);
        
        // Cancela para parar o worker
        _cancellationTokenSource.Cancel();
        
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            // Esperado quando o worker é cancelado
        }

        // Assert
        execucoes.Should().BeGreaterOrEqualTo(1);
        _motorRebalanceamentoMock.Verify(
            x => x.RebalancearPorDesvioProporcaoAsync(0.10m),
            Times.AtLeastOnce);
    }
}
