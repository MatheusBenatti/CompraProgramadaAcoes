using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Application.Interfaces.Repositories;

public interface IContaMasterRepository
{
    Task<ContaGrafica> ObterContaMasterAsync();
    Task<ContaGrafica> ObterComCustodiasAsync(long id);
    Task AddAsync(ContaGrafica conta);
    Task UpdateAsync(ContaGrafica conta);
    Task SaveChangesAsync();
}
