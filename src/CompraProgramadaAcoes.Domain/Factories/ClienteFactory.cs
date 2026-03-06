using CompraProgramadaAcoes.Domain.Entities;
using CompraProgramadaAcoes.Domain.Interfaces;

namespace CompraProgramadaAcoes.Domain.Factories;

public class ClienteFactory : IClienteFactory
{
  public Cliente Criar(string nome, string cpf, string email, decimal valorMensal)
  {
    ValidarCamposObrigatorios(nome, cpf, email, valorMensal);
    return new Cliente(nome, cpf, email, valorMensal);
  }

  private static void ValidarCamposObrigatorios(string nome, string cpf, string email, decimal valorMensal)
  {
    if (string.IsNullOrWhiteSpace(nome))
      throw new ArgumentException("Nome é obrigatório");

    if (string.IsNullOrWhiteSpace(cpf))
      throw new ArgumentException("CPF é obrigatório");

    if (string.IsNullOrWhiteSpace(email))
      throw new ArgumentException("Email é obrigatório");

    if (valorMensal < 100m)
      throw new ArgumentException("Valor mensal mínimo é R$ 100,00");
  }
}
