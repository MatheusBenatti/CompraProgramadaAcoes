using CompraProgramadaAcoes.Domain.Entities;
using CompraProgramadaAcoes.Domain.Interfaces;

namespace CompraProgramadaAcoes.Domain.Factories;

public class CustodiaFactory : ICustodiaFactory
{
    public Custodia Criar(long contaGraficaId)
    {
        return new Custodia(contaGraficaId);
    }
}
