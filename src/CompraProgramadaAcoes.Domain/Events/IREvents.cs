using CompraProgramadaAcoes.Domain.ValueObjects;
using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Domain.Events;

public record IrDeduDuroEvent(
    long ClienteId,
    Ticker Ticker,
    int Quantidade,
    ValorMonetario ValorOperacao,
    ValorMonetario ValorIR,
    DateTime DataOperacao
) : IDomainEvent
{
  public Guid EventId { get; } = Guid.NewGuid();
  public DateTime OccurredOn { get; } = DateTime.UtcNow;
  public string EventType { get; } = nameof(IrDeduDuroEvent);
}

public record IrVendaEvent(
    long ClienteId,
    Ticker Ticker,
    int Quantidade,
    ValorMonetario ValorVenda,
    ValorMonetario CustoAquisicao,
    ValorMonetario Lucro,
    ValorMonetario ValorIR,
    DateTime DataVenda
) : IDomainEvent
{
  public Guid EventId { get; } = Guid.NewGuid();
  public DateTime OccurredOn { get; } = DateTime.UtcNow;
  public string EventType { get; } = nameof(IrVendaEvent);
}

public record RebalanceamentoEvent(
    long ClienteId,
    TipoRebalanceamento Tipo,
    Ticker? TickerVendido,
    Ticker? TickerComprado,
    ValorMonetario ValorOperacao,
    DateTime DataRebalanceamento
) : IDomainEvent
{
  public Guid EventId { get; } = Guid.NewGuid();
  public DateTime OccurredOn { get; } = DateTime.UtcNow;
  public string EventType { get; } = nameof(RebalanceamentoEvent);
}
