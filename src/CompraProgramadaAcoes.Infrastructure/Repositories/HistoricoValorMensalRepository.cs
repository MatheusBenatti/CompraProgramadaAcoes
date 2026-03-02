using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CompraProgramadaAcoes.Infrastructure.Repositories;

public class HistoricoValorMensalRepository : IHistoricoValorMensalRepository
{
    private readonly ApplicationDbContext _context;

    public HistoricoValorMensalRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HistoricoValorMensal?> ObterPorClienteIdAsync(long clienteId)
    {
        return await _context.HistoricosValorMensal
            .Where(h => h.ClienteId == clienteId)
            .OrderByDescending(h => h.DataAlteracao)
            .FirstOrDefaultAsync();
    }

    public async Task<List<HistoricoValorMensal>> ObterHistoricoAsync(long clienteId)
    {
        return await _context.HistoricosValorMensal
            .Where(h => h.ClienteId == clienteId)
            .OrderByDescending(h => h.DataAlteracao)
            .ToListAsync();
    }

    public async Task<HistoricoValorMensal> AddAsync(HistoricoValorMensal historico)
    {
        await _context.HistoricosValorMensal.AddAsync(historico);
        return historico;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
