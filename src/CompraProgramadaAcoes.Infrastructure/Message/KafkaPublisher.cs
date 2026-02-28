using CompraProgramadaAcoes.Application.Interfaces;
using Confluent.Kafka;

namespace CompraProgramadaAcoes.Infrastructure.Message;

public class KafkaPublisher : IMessagePublisher
{
    private readonly IProducer<string, string> _producer;

    public KafkaPublisher(IProducer<string, string> producer)
    {
        _producer = producer;
    }

    public async Task PublishAsync(string topic, string message)
    {
        var result = await _producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = Guid.NewGuid().ToString(),
            Value = message
        });
    }
}
