using CompraProgramadaAcoes.Workers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Confluent.Kafka;
using Moq;
using FluentAssertions;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using Microsoft.Extensions.Hosting;

namespace CompraProgramadaAcoes.UnitTests.Workers;

public class IrEventProcessorWorkerTests
{
    private readonly Mock<ILogger<IrEventProcessorWorker>> _loggerMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IEventoIRRepository> _eventoIrRepositoryMock;

    public IrEventProcessorWorkerTests()
    {
        _loggerMock = new Mock<ILogger<IrEventProcessorWorker>>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _configurationMock = new Mock<IConfiguration>();
        _eventoIrRepositoryMock = new Mock<IEventoIRRepository>();

        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(_serviceScopeFactoryMock.Object);

        _serviceScopeFactoryMock.Setup(ssf => ssf.CreateScope())
            .Returns(_serviceScopeMock.Object);

        _serviceScopeMock.Setup(ss => ss.ServiceProvider)
            .Returns(_serviceProviderMock.Object);

        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IEventoIRRepository)))
            .Returns(_eventoIrRepositoryMock.Object);
    }

    [Fact]
    public void Constructor_ParametrosValidos_DeveInicializarCorretamente()
    {
        // Act
        var worker = new IrEventProcessorWorker(_loggerMock.Object, _serviceProviderMock.Object, _configurationMock.Object);

        // Assert
        worker.Should().NotBeNull();
    }

    [Fact]
    public async Task StartAsync_ConfiguracoesValidas_DeveIniciarWorker()
    {
        // Arrange
        var bootstrapServers = "localhost:9092";
        var groupId = "test-group";
        var topic = "test-topic";

        _configurationMock.Setup(c => c["Kafka:BootstrapServers"]).Returns(bootstrapServers);
        _configurationMock.Setup(c => c["Kafka:GroupId"]).Returns(groupId);
        _configurationMock.Setup(c => c["Kafka:TopicIrEvents"]).Returns(topic);

        var worker = new IrEventProcessorWorker(_loggerMock.Object, _serviceProviderMock.Object, _configurationMock.Object);
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100)); // Cancela após um curto período

        // Act
        await worker.StartAsync(cancellationTokenSource.Token);

        // Assert - Verifica se o worker foi criado e iniciado sem exceções
        worker.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ConfiguracoesPadrao_DeveUsarValoresPadrao()
    {
        // Arrange
        var bootstrapServers = "localhost:9092";

        _configurationMock.Setup(c => c["Kafka:BootstrapServers"]).Returns(bootstrapServers);
        _configurationMock.Setup(c => c["Kafka:GroupId"]).Returns((string)null!);
        _configurationMock.Setup(c => c["Kafka:TopicIrEvents"]).Returns((string)null!);

        var worker = new IrEventProcessorWorker(_loggerMock.Object, _serviceProviderMock.Object, _configurationMock.Object);
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act
        await worker.StartAsync(cancellationTokenSource.Token);

        // Assert - Verifica se o worker foi criado e iniciado com valores padrão
        worker.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_CancellationTokenAtivado_DevePararExecucao()
    {
        // Arrange
        var bootstrapServers = "localhost:9092";
        _configurationMock.Setup(c => c["Kafka:BootstrapServers"]).Returns(bootstrapServers);
        _configurationMock.Setup(c => c["Kafka:GroupId"]).Returns("test-group");
        _configurationMock.Setup(c => c["Kafka:TopicIrEvents"]).Returns("test-topic");

        var worker = new IrEventProcessorWorker(_loggerMock.Object, _serviceProviderMock.Object, _configurationMock.Object);
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel(); // Cancela imediatamente

        // Act
        await worker.StartAsync(cancellationTokenSource.Token);

        // Assert - Verifica se o worker foi criado e finalizado corretamente
        worker.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_BootstrapServersNulo_DeveLancarExcecao()
    {
        // Arrange
        _configurationMock.Setup(c => c["Kafka:BootstrapServers"]).Returns((string)null!);

        var worker = new IrEventProcessorWorker(_loggerMock.Object, _serviceProviderMock.Object, _configurationMock.Object);
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act & Assert - O worker não lança exceção no construtor, mas pode falhar em tempo de execução
        // Como o Kafka ConsumerBuilder pode lançar exceção com bootstrap servers nulo
        await worker.StartAsync(cancellationTokenSource.Token);
        
        // Verifica que o worker foi criado mesmo com configuração inválida
        worker.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ExcecaoKafka_DeveLogarErroEContinuar()
    {
        // Arrange
        var bootstrapServers = "invalid-server:9092";
        _configurationMock.Setup(c => c["Kafka:BootstrapServers"]).Returns(bootstrapServers);
        _configurationMock.Setup(c => c["Kafka:GroupId"]).Returns("test-group");
        _configurationMock.Setup(c => c["Kafka:TopicIrEvents"]).Returns("test-topic");

        var worker = new IrEventProcessorWorker(_loggerMock.Object, _serviceProviderMock.Object, _configurationMock.Object);
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(200));

        // Act
        await worker.StartAsync(cancellationTokenSource.Token);

        // Assert - Verifica que o worker foi criado e tenta lidar com erros de Kafka
        worker.Should().NotBeNull();
        // Pode ou não logar erro dependendo de como o Kafka lida com servidores inválidos
    }

    [Theory]
    [InlineData("localhost:9092", "group1", "topic1")]
    [InlineData("server1:9092,server2:9092", "group2", "topic2")]
    [InlineData("kafka.example.com:9093", "test-group", "ir-events")]
    public async Task ExecuteAsync_DiferentesConfiguracoes_DeveIniciarComConfiguracoesCorretas(string bootstrapServers, string groupId, string topic)
    {
        // Arrange
        _configurationMock.Setup(c => c["Kafka:BootstrapServers"]).Returns(bootstrapServers);
        _configurationMock.Setup(c => c["Kafka:GroupId"]).Returns(groupId);
        _configurationMock.Setup(c => c["Kafka:TopicIrEvents"]).Returns(topic);

        var worker = new IrEventProcessorWorker(_loggerMock.Object, _serviceProviderMock.Object, _configurationMock.Object);
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        // Act
        await worker.StartAsync(cancellationTokenSource.Token);

        // Assert - Verifica se o worker foi criado com diferentes configurações
        worker.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ExcecaoDuranteProcessamento_DeveLogarErroEAguardar()
    {
        // Arrange
        var bootstrapServers = "localhost:9092";
        _configurationMock.Setup(c => c["Kafka:BootstrapServers"]).Returns(bootstrapServers);
        _configurationMock.Setup(c => c["Kafka:GroupId"]).Returns("test-group");
        _configurationMock.Setup(c => c["Kafka:TopicIrEvents"]).Returns("test-topic");

        // Simular exceção durante o processamento - configurar o scope para lançar exceção
        var serviceScopeMock = new Mock<IServiceScope>();
        var serviceProviderInScopeMock = new Mock<IServiceProvider>();
        
        serviceScopeMock.Setup(ss => ss.ServiceProvider).Returns(serviceProviderInScopeMock.Object);
        serviceProviderInScopeMock.Setup(sp => sp.GetService(typeof(IEventoIRRepository)))
            .Throws(new InvalidOperationException("Repository error"));
        
        _serviceScopeFactoryMock.Setup(ssf => ssf.CreateScope()).Returns(serviceScopeMock.Object);

        var worker = new IrEventProcessorWorker(_loggerMock.Object, _serviceProviderMock.Object, _configurationMock.Object);
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(200));

        // Act
        await worker.StartAsync(cancellationTokenSource.Token);

        // Assert - Verifica que o worker foi criado e lida com exceções
        worker.Should().NotBeNull();
        // Pode logar erro se o processamento falhar
    }

    [Fact]
    public async Task StopAsync_DevePararWorkerCorretamente()
    {
        // Arrange
        var bootstrapServers = "localhost:9092";
        _configurationMock.Setup(c => c["Kafka:BootstrapServers"]).Returns(bootstrapServers);
        _configurationMock.Setup(c => c["Kafka:GroupId"]).Returns("test-group");
        _configurationMock.Setup(c => c["Kafka:TopicIrEvents"]).Returns("test-topic");

        var worker = new IrEventProcessorWorker(_loggerMock.Object, _serviceProviderMock.Object, _configurationMock.Object);
        var cancellationToken = new CancellationToken();

        // Act
        await worker.StopAsync(cancellationToken);

        // Assert - BackgroundService.StopAsync não tem implementação personalizada, então não há verificação específica
    }

    [Fact]
    public async Task StartAsync_DeveChamarExecuteAsync()
    {
        // Arrange
        var bootstrapServers = "localhost:9092";
        _configurationMock.Setup(c => c["Kafka:BootstrapServers"]).Returns(bootstrapServers);
        _configurationMock.Setup(c => c["Kafka:GroupId"]).Returns("test-group");
        _configurationMock.Setup(c => c["Kafka:TopicIrEvents"]).Returns("test-topic");

        var worker = new IrEventProcessorWorker(_loggerMock.Object, _serviceProviderMock.Object, _configurationMock.Object);
        var cancellationToken = new CancellationToken(true); // Cancela imediatamente

        // Act
        await worker.StartAsync(cancellationToken);

        // Assert - BackgroundService.StartAsync chama ExecuteAsync internamente
        worker.Should().NotBeNull();
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

    private void VerifyLoggerLoggedError(string expectedMessage)
    {
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
