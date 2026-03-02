namespace CompraProgramadaAcoes.Domain.Events;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
    string EventType { get; }
}

public interface IAggregateRoot
{
    IReadOnlyCollection<IDomainEvent> GetUncommittedEvents();
    void ClearUncommittedEvents();
}
