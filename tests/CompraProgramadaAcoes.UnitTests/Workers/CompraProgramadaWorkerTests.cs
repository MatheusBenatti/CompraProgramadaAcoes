using CompraProgramadaAcoes.Application.Interfaces;
using CompraProgramadaAcoes.Application.Workers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace CompraProgramadaAcoes.UnitTests.Workers;

public class TestableCompraProgramadaWorker : CompraProgramadaWorker
{
    public TestableCompraProgramadaWorker(
        ILogger<CompraProgramadaWorker> logger,
        IServiceProvider serviceProvider) : base(logger, serviceProvider)
    {
    }

    public new Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return base.ExecuteAsync(stoppingToken);
    }
}

public class CompraProgramadaWorkerTests
{
    private readonly Mock<ILogger<CompraProgramadaWorker>> _loggerMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IMotorCompraProgramada> _motorCompraMock;
    private readonly TestableCompraProgramadaWorker _worker;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public CompraProgramadaWorkerTests()
    {
        _loggerMock = new Mock<ILogger<CompraProgramadaWorker>>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _motorCompraMock = new Mock<IMotorCompraProgramada>();
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
            .Setup(x => x.GetService(typeof(IMotorCompraProgramada)))
            .Returns(_motorCompraMock.Object);

        _worker = new TestableCompraProgramadaWorker(_loggerMock.Object, _serviceProviderMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ComDeveExecutarHojeVerdadeiro_DeveExecutarCompras()
    {
        // Arrange
        _motorCompraMock
            .Setup(x => x.DeveExecutarHoje(It.IsAny<DateTime>()))
            .ReturnsAsync(true);

        _motorCompraMock
            .Setup(x => x.ExecutarComprasProgramadasAsync(It.IsAny<DateTime>()))
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
        _motorCompraMock.Verify(x => x.DeveExecutarHoje(It.IsAny<DateTime>()), Times.AtLeastOnce);
        _motorCompraMock.Verify(x => x.ExecutarComprasProgramadasAsync(It.IsAny<DateTime>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_ComDeveExecutarHojeFalso_NaoDeveExecutarCompras()
    {
        // Arrange
        _motorCompraMock
            .Setup(x => x.DeveExecutarHoje(It.IsAny<DateTime>()))
            .ReturnsAsync(false);

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
        _motorCompraMock.Verify(x => x.DeveExecutarHoje(It.IsAny<DateTime>()), Times.AtLeastOnce);
        _motorCompraMock.Verify(x => x.ExecutarComprasProgramadasAsync(It.IsAny<DateTime>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ComExcecao_DeveLogarErroEContinuar()
    {
        // Arrange
        var agora = new DateTime(2024, 01, 15, 10, 0, 0);
        _motorCompraMock
            .Setup(x => x.DeveExecutarHoje(It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception("Erro de teste"));

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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro durante execução do worker de compra programada")),
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
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Parando worker de compra programada")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
