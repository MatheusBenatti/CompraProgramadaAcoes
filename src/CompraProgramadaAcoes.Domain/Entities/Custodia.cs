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

  public void AtualizarQuantidade(int quantidade)
  {
    Quantidade = quantidade;
    DataUltimaAtualizacao = DateTime.UtcNow;
  }

  public void AtualizarPrecoMedio(decimal precoMedio)
  {
    PrecoMedio = precoMedio;
    DataUltimaAtualizacao = DateTime.UtcNow;
  }

  public void AtualizarTicker(string ticker)
  {
    Ticker = ticker;
    DataUltimaAtualizacao = DateTime.UtcNow;
  }

  public void AtualizarCustodia(int quantidade, decimal precoMedio, string? ticker = null)
  {
    Quantidade = quantidade;
    PrecoMedio = precoMedio;
    if (ticker != null)
    {
      Ticker = ticker;
    }
    DataUltimaAtualizacao = DateTime.UtcNow;
  }
}
