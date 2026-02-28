using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CompraProgramadaAcoes.Infrastructure.Message;

public class KafkaConsumerBackgroundService : BackgroundService
{
  private readonly ILogger<KafkaConsumerBackgroundService> _logger;
  private readonly ConsumerConfig _consumerConfig;
  private readonly string _topic;

  public KafkaConsumerBackgroundService(
    ILogger<KafkaConsumerBackgroundService> logger,
    IOptions<KafkaSettings> options)
  {
    _logger = logger;
    var settings = options.Value;
    _consumerConfig = new ConsumerConfig
    {
      GroupId = settings.GroupId,
      BootstrapServers = settings.BootstrapServers,
      AutoOffsetReset = AutoOffsetReset.Earliest
    };

    _topic = settings.Topic;
  }

  protected override Task ExecuteAsync(CancellationToken stoppingToken)
  {

    using var consumer = new ConsumerBuilder<string, string>(_consumerConfig).Build();
    consumer.Subscribe([_topic]);
    try
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        var consumeResult = consumer.Consume(stoppingToken);
        _logger.LogInformation("Consumed message: {Message}", consumeResult.Message.Value);
      }
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("Kafka consumer stopping...");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error in Kafka consumer");
    }
    finally
    {
      consumer.Close();
    }
    return Task.CompletedTask;
  }
}
