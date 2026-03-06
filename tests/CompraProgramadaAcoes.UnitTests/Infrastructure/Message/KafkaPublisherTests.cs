using CompraProgramadaAcoes.Infrastructure.Message;
using Confluent.Kafka;
using Moq;
using FluentAssertions;

namespace CompraProgramadaAcoes.UnitTests.Infrastructure.Message;

public class KafkaPublisherTests
{
    private readonly Mock<IProducer<string, string>> _producerMock;
    private readonly KafkaPublisher _publisher;

    public KafkaPublisherTests()
    {
        _producerMock = new Mock<IProducer<string, string>>();
        _publisher = new KafkaPublisher(_producerMock.Object);
    }

    [Fact]
    public async Task PublishAsync_TopicAndMessageValidos_DeveChamarProduceAsync()
    {
        // Arrange
        var topic = "test_topic";
        var message = "test_message";

        _producerMock.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeliveryResult<string, string>());

        // Act
        await _publisher.PublishAsync(topic, message);

        // Assert
        _producerMock.Verify(p => p.ProduceAsync(
            topic,
            It.Is<Message<string, string>>(m => m.Value == message && !string.IsNullOrEmpty(m.Key)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_TopicVazio_DeveLancarExcecao()
    {
        // Arrange
        var topic = "";
        var message = "test_message";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _publisher.PublishAsync(topic, message));
    }

    [Fact]
    public async Task PublishAsync_MessageVazio_DeveChamarProduceAsync()
    {
        // Arrange
        var topic = "test_topic";
        var message = "";

        _producerMock.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeliveryResult<string, string>());

        // Act
        await _publisher.PublishAsync(topic, message);

        // Assert
        _producerMock.Verify(p => p.ProduceAsync(
            topic,
            It.Is<Message<string, string>>(m => m.Value == message),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_MessageNulo_DeveChamarProduceAsync()
    {
        // Arrange
        var topic = "test_topic";
        string? message = null;

        _producerMock.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeliveryResult<string, string>());

        // Act
        await _publisher.PublishAsync(topic, message!);

        // Assert
        _producerMock.Verify(p => p.ProduceAsync(
            topic,
            It.Is<Message<string, string>>(m => m.Value == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ProducerLancaExcecao_DevePropagarExcecao()
    {
        // Arrange
        var topic = "test_topic";
        var message = "test_message";
        var expectedException = new KafkaException(new Error(ErrorCode.Local_AllBrokersDown));

        _producerMock.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KafkaException>(() => _publisher.PublishAsync(topic, message));
        exception.Should().Be(expectedException);
    }

    [Theory]
    [InlineData("topic1", "message1")]
    [InlineData("topic2", "message2")]
    [InlineData("topic_with_underscore", "message with spaces")]
    public async Task PublishAsync_DiferentesTopicsEMessages_DeveChamarProduceAsyncComParametrosCorretos(string topic, string message)
    {
        // Arrange
        _producerMock.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeliveryResult<string, string>());

        // Act
        await _publisher.PublishAsync(topic, message);

        // Assert
        _producerMock.Verify(p => p.ProduceAsync(
            topic,
            It.Is<Message<string, string>>(m => m.Value == message),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_MultiplasChamadas_DeveGerarChavesDiferentes()
    {
        // Arrange
        var topic = "test_topic";
        var message = "test_message";
        var capturedKeys = new List<string>();

        _producerMock.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()))
            .Callback<string, Message<string, string>, CancellationToken>((t, m, ct) => capturedKeys.Add(m.Key!))
            .ReturnsAsync(new DeliveryResult<string, string>());

        // Act
        await _publisher.PublishAsync(topic, message);
        await _publisher.PublishAsync(topic, message);
        await _publisher.PublishAsync(topic, message);

        // Assert
        capturedKeys.Should().HaveCount(3);
        capturedKeys.Distinct().Should().HaveCount(3); // Todas as chaves devem ser diferentes
        capturedKeys.All(k => Guid.TryParse(k, out _)).Should().BeTrue(); // Todas devem ser GUIDs válidos
    }

    [Fact]
    public async Task PublishAsync_DeliveryResultComStatus_DeveRetornarNormalmente()
    {
        // Arrange
        var topic = "test_topic";
        var message = "test_message";
        var deliveryResult = new DeliveryResult<string, string>
        {
            Status = PersistenceStatus.Persisted,
            Topic = topic,
            Partition = new Partition(0),
            Offset = new Offset(123)
        };

        _producerMock.Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(deliveryResult);

        // Act & Assert - Não deve lançar exceção
        await _publisher.PublishAsync(topic, message);

        _producerMock.Verify(p => p.ProduceAsync(
            topic,
            It.Is<Message<string, string>>(m => m.Value == message),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
