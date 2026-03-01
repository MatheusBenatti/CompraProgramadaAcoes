namespace CompraProgramadaAcoes.Domain.Entities;

public class Cliente(string nome, string cpf, string email, decimal valorMensal)
{
  public long Id { get; private set; }
  public string Nome { get; private set; } = nome;
  public string Cpf { get; private set; } = cpf;
  public string Email { get; private set; } = email;
  public decimal ValorMensal { get; private set; } = valorMensal;
  public bool Ativo { get; private set; } = true;
  public DateTime DataAdesao { get; private set; } = DateTime.UtcNow;
  public ContaGrafica ContaGrafica { get; private set; } = null!;
  public Custodia Custodia { get; private set; } = null!;

  public void AssociarContaGrafica(ContaGrafica contaGrafica)
  {
    ContaGrafica = contaGrafica;
  }
  
  public void AssociarCustodia(Custodia custodia)
  {
    Custodia = custodia;
  }
}
