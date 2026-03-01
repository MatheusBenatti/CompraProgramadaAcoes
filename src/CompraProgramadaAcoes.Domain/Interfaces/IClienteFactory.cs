using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Domain.Interfaces;

public interface IClienteFactory
{
  Cliente Criar(string nome, string cpf, string email, decimal valorMensal);
}
