using CompraProgramadaAcoes.Application.Interfaces;
using Confluent.Kafka;

namespace CompraProgramadaAcoes.Infrastructure.Message;

public class KafkaPublisher(IProducer<string, string> producer) : IMessagePublisher
{
  private readonly IProducer<string, string> _producer = producer;

  public async Task PublishAsync(string topic, string message)
  {
    await _producer.ProduceAsync(topic, new Message<string, string>
    {
      Key = Guid.NewGuid().ToString(),
      Value = message
    });
  }
}
