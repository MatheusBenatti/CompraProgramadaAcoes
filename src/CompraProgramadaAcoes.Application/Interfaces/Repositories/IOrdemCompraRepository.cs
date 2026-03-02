using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Application.Interfaces.Repositories;

public interface IOrdemCompraRepository
{
    Task<OrdemCompra?> ObterPorIdAsync(long id);
    Task<List<OrdemCompra>> ObterPorContaMasterAsync(long contaMasterId);
    Task<List<OrdemCompra>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim);
    Task AddAsync(OrdemCompra ordem);
    Task UpdateAsync(OrdemCompra ordem);
    Task SaveChangesAsync();
}
