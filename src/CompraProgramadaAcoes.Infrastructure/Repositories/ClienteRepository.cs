using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CompraProgramadaAcoes.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly ApplicationDbContext _context;

    public ClienteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Cliente?> GetByIdAsync(long id)
    {
        return await _context.Clientes
            .Include(c => c.ContaGrafica)
            .ThenInclude(cg => cg.Custodias)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Cliente?> GetByCpfAsync(string cpf)
    {
        return await _context.Clientes
            .FirstOrDefaultAsync(c => c.Cpf == cpf);
    }

    public async Task<bool> CpfExistsAsync(string cpf)
    {
        return await _context.Clientes
            .AnyAsync(c => c.Cpf == cpf);
    }

    public async Task<List<Cliente>> ObterClientesAtivosAsync()
    {
        return await _context.Clientes
            .Include(c => c.ContaGrafica)
            .ThenInclude(cg => cg.Custodias)
            .Where(c => c.Ativo)
            .ToListAsync();
    }

    public async Task<Cliente> AddAsync(Cliente cliente)
    {
        await _context.Clientes.AddAsync(cliente);
        return cliente;
    }

    public async Task UpdateAsync(Cliente cliente)
    {
        _context.Clientes.Update(cliente);
        await Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
