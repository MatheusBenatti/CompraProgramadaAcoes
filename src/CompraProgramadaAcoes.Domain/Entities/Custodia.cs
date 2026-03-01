namespace CompraProgramadaAcoes.Domain.Entities;

public class Custodia(int clienteId, int contaGraficaId)
{
  public int Id { get; private set; }
  public DateTime DataCriacao { get; private set; } = DateTime.UtcNow;
  public decimal ValorTotal { get; private set; } = 0;
  public int ClienteId { get; private set; } = clienteId;
  public Cliente Cliente { get; private set; } = null!;
  public int ContaGraficaId { get; private set; } = contaGraficaId;
  public ContaGrafica ContaGrafica { get; private set; } = null!;

  public void AtualizarValor(decimal valor)
  {
    ValorTotal = valor;
  }
}
