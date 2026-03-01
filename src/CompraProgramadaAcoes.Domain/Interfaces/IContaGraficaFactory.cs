using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Domain.Interfaces;

public interface IContaGraficaFactory
{
  ContaGrafica Criar(int clienteId);
}
