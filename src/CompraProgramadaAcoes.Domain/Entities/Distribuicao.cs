namespace CompraProgramadaAcoes.Domain.Entities;

public class Distribuicao(long ordemCompraId, long custodiaFilhoteId, string ticker, int quantidade, decimal precoUnitario)
{
  public long Id { get; private set; }
  public long OrdemCompraId { get; private set; } = ordemCompraId;
  public long CustodiaFilhoteId { get; private set; } = custodiaFilhoteId;
  public string Ticker { get; private set; } = ticker;
  public int Quantidade { get; private set; } = quantidade;
  public decimal PrecoUnitario { get; private set; } = precoUnitario;
  public DateTime DataDistribuicao { get; private set; } = DateTime.UtcNow;

  public OrdemCompra OrdemCompra { get; private set; } = null!;
  public Custodia CustodiaFilhote { get; private set; } = null!;

  public void AtualizarOrdemCompraId(long ordemCompraId)
  {
    OrdemCompraId = ordemCompraId;
  }

  public void AtualizarTicker(string ticker)
  {
    Ticker = ticker;
  }

  public void AtualizarQuantidade(int quantidade)
  {
    Quantidade = quantidade;
  }

  public void AtualizarPrecoUnitario(decimal precoUnitario)
  {
    PrecoUnitario = precoUnitario;
  }

  public void AtualizarDataDistribuicao(DateTime dataDistribuicao)
  {
    DataDistribuicao = dataDistribuicao;
  }
}
