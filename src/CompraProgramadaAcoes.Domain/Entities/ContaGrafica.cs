namespace CompraProgramadaAcoes.Domain.Entities;

public class ContaGrafica(long? clienteId)
{
  public long Id { get; private set; }
  public string? NumeroConta { get; private set; }
  public string Tipo { get; private set; } = "FILHOTE";
  public DateTime DataCriacao { get; private set; } = DateTime.UtcNow;
  public long? ClienteId { get; private set; } = clienteId;
  public Cliente? Cliente { get; private set; }
  public ICollection<Custodia> Custodias { get; private set; } = [];
  public ICollection<OrdemCompra> OrdensCompra { get; set; } = [];

  public void GerarNumeroConta()
  {
    if (Id <= 0)
      throw new InvalidOperationException("ID não disponível para gerar número da conta");

    NumeroConta = $"FLH-{DateTime.UtcNow:yyyyMMdd}-{Id:D6}";
  }

  public void AtualizarTipo(string tipo)
  {
    Tipo = tipo;
  }

  public void AtualizarNumeroConta(string numeroConta)
  {
    NumeroConta = numeroConta;
  }
}
