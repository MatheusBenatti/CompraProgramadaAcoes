using Microsoft.EntityFrameworkCore;
using CompraProgramadaAcoes.Domain.Entities;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;

namespace CompraProgramadaAcoes.Infrastructure.Persistence.Repositories;

public class ClienteRepository(AppDbContext context) : IClienteRepository
{
    private readonly AppDbContext _context = context;

  public async Task<Cliente?> GetByIdAsync(int id)
    {
        return await _context.Clientes
            .Include(c => c.ContaGrafica)
            .Include(c => c.Custodia)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Cliente?> GetByCpfAsync(string cpf)
    {
        return await _context.Clientes
            .Include(c => c.ContaGrafica)
            .Include(c => c.Custodia)
            .FirstOrDefaultAsync(c => c.Cpf == cpf);
    }

    public async Task<bool> CpfExistsAsync(string cpf)
    {
        return await _context.Clientes.AnyAsync(c => c.Cpf == cpf);
    }

    public async Task<Cliente> AddAsync(Cliente cliente)
    {
        await _context.Clientes.AddAsync(cliente);
        return cliente;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
