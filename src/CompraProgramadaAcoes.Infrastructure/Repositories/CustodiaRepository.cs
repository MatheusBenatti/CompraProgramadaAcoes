using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CompraProgramadaAcoes.Infrastructure.Repositories;

public class CustodiaRepository : ICustodiaRepository
{
    private readonly ApplicationDbContext _context;

    public CustodiaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Custodia>> ObterPorContaGraficaAsync(long contaGraficaId)
    {
        return await _context.Custodias
            .Where(c => c.ContaGraficaId == contaGraficaId)
            .ToListAsync();
    }

    public async Task<List<Custodia>> ObterPorContaMasterAsync(long contaMasterId)
    {
        return await _context.Custodias
            .Where(c => c.ContaGraficaId == contaMasterId)
            .ToListAsync();
    }

    public async Task<Custodia?> ObterPorTickerAsync(long contaGraficaId, string ticker)
    {
        return await _context.Custodias
            .FirstOrDefaultAsync(c => c.ContaGraficaId == contaGraficaId && c.Ticker == ticker);
    }

    public async Task<Custodia?> ObterPorTickerMasterAsync(long contaMasterId, string ticker)
    {
        return await _context.Custodias
            .FirstOrDefaultAsync(c => c.ContaGraficaId == contaMasterId && c.Ticker == ticker);
    }

    public async Task AddAsync(Custodia custodia)
    {
        await _context.Custodias.AddAsync(custodia);
    }

    public async Task UpdateAsync(Custodia custodia)
    {
        _context.Custodias.Update(custodia);
        await Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
