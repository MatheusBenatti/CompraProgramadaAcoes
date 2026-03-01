using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Application.Interfaces.Repositories;

public interface ICustodiaRepository
{
    Task<Custodia?> GetByIdAsync(int id);
    Task<Custodia> AddAsync(Custodia custodia);
    Task<int> SaveChangesAsync();
}
