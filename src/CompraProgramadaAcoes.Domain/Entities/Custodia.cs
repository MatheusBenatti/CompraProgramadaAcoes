namespace CompraProgramadaAcoes.Domain.Entities;

public class Custodia(long clienteId, long contaGraficaId)
{
  public long Id { get; private set; }
  public string Ticker { get; private set; } = null!;
  public int Quantidade { get; private set; } = 0;
  public decimal PrecoMedio { get; private set; } = 0;
  public decimal ValorTotal { get; private set; } = 0;
  public DateTime DataUltimaAtualizacao { get; private set; } = DateTime.UtcNow;
  public long ClienteId { get; private set; } = clienteId;
  public Cliente Cliente { get; private set; } = null!;
  public long ContaGraficaId { get; private set; } = contaGraficaId;
  public ContaGrafica ContaGrafica { get; private set; } = null!;

  public void AtualizarValor(decimal valor)
  {
    ValorTotal = valor;
  }
}
