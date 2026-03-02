using CompraProgramadaAcoes.Domain.Events;

namespace CompraProgramadaAcoes.Domain.Services;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event) where T : IDomainEvent;
    Task PublishAsync(IDomainEvent @event);
    Task PublishBatchAsync(IEnumerable<IDomainEvent> events);
}
