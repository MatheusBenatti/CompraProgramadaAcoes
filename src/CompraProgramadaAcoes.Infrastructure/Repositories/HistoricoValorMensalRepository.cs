using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Domain.Entities;
using CompraProgramadaAcoes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CompraProgramadaAcoes.Infrastructure.Repositories;

public class HistoricoValorMensalRepository(AppDbContext context) : IHistoricoValorMensalRepository
{
  private readonly AppDbContext _context = context;

  public async Task<HistoricoValorMensal?> ObterPorClienteIdAsync(long clienteId)
  {
    return await _context.HistoricoValoresMensais
        .Where(h => h.ClienteId == clienteId)
        .OrderByDescending(h => h.DataAlteracao)
        .FirstOrDefaultAsync();
  }

  public async Task<List<HistoricoValorMensal>> ObterHistoricoAsync(long clienteId)
  {
    return await _context.HistoricoValoresMensais
        .Where(h => h.ClienteId == clienteId)
        .OrderByDescending(h => h.DataAlteracao)
        .ToListAsync();
  }

  public async Task<HistoricoValorMensal> AddAsync(HistoricoValorMensal historico)
  {
    await _context.HistoricoValoresMensais.AddAsync(historico);
    return historico;
  }

  public async Task<int> SaveChangesAsync()
  {
    return await _context.SaveChangesAsync();
  }
}
