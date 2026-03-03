using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Domain.Entities;
using CompraProgramadaAcoes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CompraProgramadaAcoes.Infrastructure.Repositories;

public class ContaGraficaRepository(AppDbContext context) : IContaGraficaRepository
{
    private readonly AppDbContext _context = context;

  public async Task<ContaGrafica?> GetByIdAsync(long id)
    {
        return await _context.ContasGraficas
            .Include(cg => cg.Cliente)
            .Include(cg => cg.Custodias)
            .FirstOrDefaultAsync(cg => cg.Id == id);
    }

    public async Task<List<ContaGrafica>> ObterPorTipoAsync(string tipo)
    {
        return await _context.ContasGraficas
            .Include(cg => cg.Cliente)
            .Include(cg => cg.Custodias)
            .Where(cg => cg.Tipo == tipo)
            .ToListAsync();
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
