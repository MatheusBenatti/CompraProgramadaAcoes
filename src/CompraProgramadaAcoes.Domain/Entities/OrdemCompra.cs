namespace CompraProgramadaAcoes.Domain.Entities;

public class OrdemCompra(long contaMasterId, string ticker, int quantidade, decimal precoUnitario, TipoMercado tipoMercado)
{
  public long Id { get; private set; }
  public long ContaMasterId { get; private set; } = contaMasterId;
  public string Ticker { get; private set; } = ticker;
  public int Quantidade { get; private set; } = quantidade;
  public decimal PrecoUnitario { get; private set; } = precoUnitario;
  public TipoMercado TipoMercado { get; private set; } = tipoMercado;
  public DateTime DataExecucao { get; private set; } = DateTime.UtcNow;
  public ICollection<Distribuicao> Distribuicoes { get; private set; } = [];

  public ContaGrafica ContaMaster { get; private set; } = null!;

  public void AtualizarPrecoUnitario(decimal precoUnitario)
  {
    PrecoUnitario = precoUnitario;
  }

  public void AtualizarTipoMercado(TipoMercado tipoMercado)
  {
    TipoMercado = tipoMercado;
  }

  public void AtualizarDataExecucao(DateTime dataExecucao)
  {
    DataExecucao = dataExecucao;
  }
}

public enum TipoMercado
{
  Lote,
  Fracionario
}
