using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Application.Interfaces.Repositories;

public interface IContaGraficaRepository
{
    Task<ContaGrafica?> GetByIdAsync(int id);
    Task<ContaGrafica> AddAsync(ContaGrafica contaGrafica);
    Task<int> SaveChangesAsync();
}
