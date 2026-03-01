using Microsoft.EntityFrameworkCore;
using CompraProgramadaAcoes.Domain.Entities;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;

namespace CompraProgramadaAcoes.Infrastructure.Persistence.Repositories;

public class CustodiaRepository(AppDbContext context) : ICustodiaRepository
{
    private readonly AppDbContext _context = context;

  public async Task<Custodia?> GetByIdAsync(int id)
    {
        return await _context.Custodias
            .Include(c => c.Cliente)
            .Include(c => c.ContaGrafica)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Custodia> AddAsync(Custodia custodia)
    {
        await _context.Custodias.AddAsync(custodia);
        return custodia;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
