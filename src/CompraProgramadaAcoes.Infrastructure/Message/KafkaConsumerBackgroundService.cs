using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CompraProgramadaAcoes.Infrastructure.Message;

public class KafkaConsumerBackgroundService : BackgroundService
{
    private readonly ILogger<KafkaConsumerBackgroundService> _logger;
    private readonly ConsumerConfig _consumerConfig;

    public KafkaConsumerBackgroundService(ILogger<KafkaConsumerBackgroundService> logger)
    {
        _logger = logger;
        _consumerConfig = new ConsumerConfig
        {
            GroupId = "programmed-purchase-group",
            BootstrapServers = "kafka:9092",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var consumer = new ConsumerBuilder<string, string>(_consumerConfig).Build();
            consumer.Subscribe(new[] { "programmed-purchases" });

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);
                    _logger.LogInformation($"Consumed message: {consumeResult.Message.Value}");
                    await Task.Delay(100, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            consumer.Close();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in Kafka consumer: {ex.Message}");
        }
    }
}
