using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Application.Interfaces.Repositories;

public interface IClienteRepository
{
    Task<Cliente?> GetByIdAsync(long id);
    Task<Cliente?> GetByCpfAsync(string cpf);
    Task<bool> CpfExistsAsync(string cpf);
    Task<List<Cliente>> ObterClientesAtivosAsync();
    Task<Cliente> AddAsync(Cliente cliente);
    Task UpdateAsync(Cliente cliente);
    Task<int> SaveChangesAsync();
}
