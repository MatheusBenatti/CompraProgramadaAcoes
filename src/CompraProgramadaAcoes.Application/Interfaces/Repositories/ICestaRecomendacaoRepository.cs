using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Application.Interfaces.Repositories;

public interface ICestaRecomendacaoRepository
{
    Task<CestaRecomendacao?> ObterCestaVigenteAsync();
    Task<List<CestaRecomendacao>> ObterHistoricoAsync();
    Task<CestaRecomendacao?> ObterPorIdAsync(long id);
    Task AddAsync(CestaRecomendacao cesta);
    Task UpdateAsync(CestaRecomendacao cesta);
    Task SaveChangesAsync();
}
