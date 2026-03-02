using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Application.Interfaces.Repositories;

public interface IDistribuicaoRepository
{
    Task<Distribuicao?> ObterPorIdAsync(long id);
    Task<List<Distribuicao>> ObterPorOrdemCompraAsync(long ordemCompraId);
    Task<List<Distribuicao>> ObterPorClienteAsync(long clienteId);
    Task<List<Distribuicao>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim);
    Task AddAsync(Distribuicao distribuicao);
    Task UpdateAsync(Distribuicao distribuicao);
    Task SaveChangesAsync();
}
