using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Application.Interfaces.Repositories;

public interface IContaGraficaRepository
{
    Task<ContaGrafica?> GetByIdAsync(long id);
    Task<List<ContaGrafica>> ObterPorTipoAsync(string tipo);
    Task<ContaGrafica> AddAsync(ContaGrafica contaGrafica);
    Task<int> SaveChangesAsync();
}
