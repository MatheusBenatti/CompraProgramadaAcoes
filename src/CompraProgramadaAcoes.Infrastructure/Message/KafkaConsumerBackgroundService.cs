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

    _logger.LogInformation("Kafka configuration - BootstrapServers: {BootstrapServers}, GroupId: {GroupId}, Topic: {Topic}",
      settings.BootstrapServers, settings.GroupId, settings.Topic);

    _consumerConfig = new ConsumerConfig
    {
      GroupId = settings.GroupId,
      BootstrapServers = settings.BootstrapServers,
      AutoOffsetReset = AutoOffsetReset.Earliest
    };

    _topic = settings.Topic;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("Starting Kafka consumer with BootstrapServers: {BootstrapServers}", _consumerConfig.BootstrapServers);

    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        _logger.LogInformation("Attempting to connect to Kafka at {BootstrapServers}...", _consumerConfig.BootstrapServers);

        using var consumer = new ConsumerBuilder<string, string>(_consumerConfig).Build();
        consumer.Subscribe(new[] { _topic });

        _logger.LogInformation("Kafka consumer connected successfully to {BootstrapServers}", _consumerConfig.BootstrapServers);

        while (!stoppingToken.IsCancellationRequested)
        {
          var consumeResult = consumer.Consume(stoppingToken);
          _logger.LogInformation("Consumed message: {Message}", consumeResult.Message.Value);
        }

        consumer.Close();
      }
      catch (OperationCanceledException)
      {
        _logger.LogInformation("Kafka consumer stopping...");
        break;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error connecting to Kafka at {BootstrapServers}. Retrying in 5 seconds...", _consumerConfig.BootstrapServers);

        // Espera para nova tentativa
        try
        {
          await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
        catch (OperationCanceledException)
        {
          break;
        }
      }
    }
  }
}
