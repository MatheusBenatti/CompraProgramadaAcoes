using CompraProgramadaAcoes.Domain.ValueObjects;

namespace CompraProgramadaAcoes.Domain.Events;

public record ClienteCriadoEvent(
    Guid ClienteId,
    string Nome,
    CPF Cpf,
    Email Email,
    ValorMonetario ValorMensal,
    DateTime DataAdesao
) : IDomainEvent
{
  public Guid EventId { get; } = Guid.NewGuid();
  public DateTime OccurredOn { get; } = DateTime.UtcNow;
  public string EventType { get; } = nameof(ClienteCriadoEvent);
}

public record ClienteDesativadoEvent(
    Guid ClienteId,
    string Motivo,
    DateTime DataDesativacao
) : IDomainEvent
{
  public Guid EventId { get; } = Guid.NewGuid();
  public DateTime OccurredOn { get; } = DateTime.UtcNow;
  public string EventType { get; } = nameof(ClienteDesativadoEvent);
}

public record ValorMensalAlteradoEvent(
    Guid ClienteId,
    ValorMonetario ValorAnterior,
    ValorMonetario ValorNovo,
    DateTime DataAlteracao
) : IDomainEvent
{
  public Guid EventId { get; } = Guid.NewGuid();
  public DateTime OccurredOn { get; } = DateTime.UtcNow;
  public string EventType { get; } = nameof(ValorMensalAlteradoEvent);
}
