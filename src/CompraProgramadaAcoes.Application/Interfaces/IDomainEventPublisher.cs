using CompraProgramadaAcoes.Domain.Events;

namespace CompraProgramadaAcoes.Application.Interfaces;

public interface IDomainEventPublisher
{
    Task PublishAsync<T>(T @event) where T : IDomainEvent;
    Task PublishAsync(IDomainEvent @event);
    Task PublishBatchAsync(IEnumerable<IDomainEvent> events);
}
