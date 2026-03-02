using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Application.Interfaces.Repositories;

public interface IHistoricoValorMensalRepository
{
    Task<HistoricoValorMensal> AddAsync(HistoricoValorMensal historico);
    Task<int> SaveChangesAsync();
}
