using CompraProgramadaAcoes.Domain.Entities;
using CompraProgramadaAcoes.Domain.Interfaces;

namespace CompraProgramadaAcoes.Domain.Factories;

public class ContaGraficaFactory : IContaGraficaFactory
{
    public ContaGrafica Criar(int clienteId)
    {
        var conta = new ContaGrafica(clienteId);
        return conta;
    }
}
