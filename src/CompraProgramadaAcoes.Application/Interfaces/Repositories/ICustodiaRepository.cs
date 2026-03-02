using CompraProgramadaAcoes.Domain.Entities;
using System.Collections.Generic;

namespace CompraProgramadaAcoes.Application.Interfaces.Repositories;

public interface ICustodiaRepository
{
    Task<List<Custodia>> ObterPorContaGraficaAsync(long contaGraficaId);
    Task<List<Custodia>> ObterPorContaMasterAsync(long contaMasterId);
    Task<Custodia?> ObterPorTickerAsync(long contaGraficaId, string ticker);
    Task<Custodia?> ObterPorTickerMasterAsync(long contaMasterId, string ticker);
    Task AddAsync(Custodia custodia);
    Task UpdateAsync(Custodia custodia);
    Task<int> SaveChangesAsync();
}
