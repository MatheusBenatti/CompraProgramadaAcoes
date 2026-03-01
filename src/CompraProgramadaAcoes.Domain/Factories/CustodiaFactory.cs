using CompraProgramadaAcoes.Domain.Entities;
using CompraProgramadaAcoes.Domain.Interfaces;

namespace CompraProgramadaAcoes.Domain.Factories;

public class CustodiaFactory : ICustodiaFactory
{
    public Custodia Criar(int clienteId, int contaGraficaId)
    {
        return new Custodia(clienteId, contaGraficaId);
    }
}
