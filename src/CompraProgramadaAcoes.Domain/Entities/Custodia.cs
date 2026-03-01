namespace CompraProgramadaAcoes.Domain.Entities;

public class Custodia(long contaGraficaId)
{
  public long Id { get; private set; }
  public long ContaGraficaId { get; private set; } = contaGraficaId;
  public ContaGrafica ContaGrafica { get; private set; } = null!;
  public string? Ticker { get; private set; }
  public int Quantidade { get; private set; } = 0;
  public decimal PrecoMedio { get; private set; } = 0;
  public DateTime DataUltimaAtualizacao { get; private set; } = DateTime.UtcNow;
}
