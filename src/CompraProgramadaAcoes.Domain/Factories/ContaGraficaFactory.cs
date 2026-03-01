using CompraProgramadaAcoes.Domain.Entities;
using CompraProgramadaAcoes.Domain.Interfaces;

namespace CompraProgramadaAcoes.Domain.Factories;

public class ContaGraficaFactory : IContaGraficaFactory
{
    public ContaGrafica Criar(long clienteId)
    {
        var conta = new ContaGrafica(clienteId);
        return conta;
    }
}
