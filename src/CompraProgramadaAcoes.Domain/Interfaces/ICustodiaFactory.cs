using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Domain.Interfaces;

public interface ICustodiaFactory
{
    Custodia Criar(long clienteId, long contaGraficaId);
}
