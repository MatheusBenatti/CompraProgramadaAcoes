namespace CompraProgramadaAcoes.Domain.Entities;

public class Rebalanceamento(long clienteId, TipoRebalanceamento tipo, string tickerVendido, string tickerComprado, decimal valorVenda)
{
  public long Id { get; private set; }
  public long ClienteId { get; private set; } = clienteId;
  public TipoRebalanceamento Tipo { get; private set; } = tipo;
  public string TickerVendido { get; private set; } = tickerVendido;
  public string TickerComprado { get; private set; } = tickerComprado;
  public decimal ValorVenda { get; private set; } = valorVenda;
  public DateTime DataRebalanceamento { get; private set; } = DateTime.UtcNow;

  public Cliente Cliente { get; private set; } = null!;
}

public enum TipoRebalanceamento
{
  MudancaCesta,
  Desvio
}
