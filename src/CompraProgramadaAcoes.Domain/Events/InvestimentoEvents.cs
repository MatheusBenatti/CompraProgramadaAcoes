using CompraProgramadaAcoes.Domain.ValueObjects;

namespace CompraProgramadaAcoes.Domain.Events;

public record InvestimentoRealizadoEvent(
    long ClienteId,
    ValorMonetario ValorInvestido,
    Dictionary<Ticker, int> AtivosDistribuidos,
    DateTime DataInvestimento
) : IDomainEvent
{
  public Guid EventId { get; } = Guid.NewGuid();
  public DateTime OccurredOn { get; } = DateTime.UtcNow;
  public string EventType { get; } = nameof(InvestimentoRealizadoEvent);
}

public record CompraConsolidadaEvent(
    long ContaMasterId,
    Dictionary<Ticker, (int Quantidade, decimal ValorTotal)> ComprasRealizadas,
    ValorMonetario ValorTotalConsolidado,
    DateTime DataCompra
) : IDomainEvent
{
  public Guid EventId { get; } = Guid.NewGuid();
  public DateTime OccurredOn { get; } = DateTime.UtcNow;
  public string EventType { get; } = nameof(CompraConsolidadaEvent);
}

public record AtivosDistribuidosEvent(
    long ClienteId,
    string NumeroConta,
    Dictionary<Ticker, int> AtivosRecebidos,
    ValorMonetario ValorTotalDistribuido,
    DateTime DataDistribuicao
) : IDomainEvent
{
  public Guid EventId { get; } = Guid.NewGuid();
  public DateTime OccurredOn { get; } = DateTime.UtcNow;
  public string EventType { get; } = nameof(AtivosDistribuidosEvent);
}
