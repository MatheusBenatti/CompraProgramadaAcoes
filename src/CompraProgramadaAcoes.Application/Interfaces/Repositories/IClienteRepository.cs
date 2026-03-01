using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Application.Interfaces.Repositories;

public interface IClienteRepository
{
    Task<Cliente?> GetByIdAsync(int id);
    Task<Cliente?> GetByCpfAsync(string cpf);
    Task<bool> CpfExistsAsync(string cpf);
    Task<Cliente> AddAsync(Cliente cliente);
    Task<int> SaveChangesAsync();
}
