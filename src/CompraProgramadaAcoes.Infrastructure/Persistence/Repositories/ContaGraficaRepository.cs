using Microsoft.EntityFrameworkCore;
using CompraProgramadaAcoes.Domain.Entities;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;

namespace CompraProgramadaAcoes.Infrastructure.Persistence.Repositories;

public class ContaGraficaRepository(AppDbContext context) : IContaGraficaRepository
{
    private readonly AppDbContext _context = context;

  public async Task<ContaGrafica?> GetByIdAsync(int id)
    {
        return await _context.ContasGraficas
            .Include(cg => cg.Cliente)
            .FirstOrDefaultAsync(cg => cg.Id == id);
    }

    public async Task<ContaGrafica> AddAsync(ContaGrafica contaGrafica)
    {
        await _context.ContasGraficas.AddAsync(contaGrafica);
        return contaGrafica;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
