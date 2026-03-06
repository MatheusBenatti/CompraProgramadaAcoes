using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Domain.Entities;
using CompraProgramadaAcoes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CompraProgramadaAcoes.Infrastructure.Repositories;

public class OrdemCompraRepository(AppDbContext context) : IOrdemCompraRepository
{
  private readonly AppDbContext _context = context;

  public async Task<OrdemCompra?> ObterPorIdAsync(long id)
  {
    return await _context.OrdensCompra
        .Include(o => o.Distribuicoes)
        .FirstOrDefaultAsync(o => o.Id == id);
  }

  public async Task<List<OrdemCompra>> ObterPorContaMasterAsync(long contaMasterId)
  {
    return await _context.OrdensCompra
        .Include(o => o.Distribuicoes)
        .Where(o => o.ContaMasterId == contaMasterId)
        .OrderByDescending(o => o.DataExecucao)
        .ToListAsync();
  }

  public async Task<List<OrdemCompra>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim)
  {
    return await _context.OrdensCompra
        .Include(o => o.Distribuicoes)
        .Where(o => o.DataExecucao >= inicio && o.DataExecucao <= fim)
        .OrderByDescending(o => o.DataExecucao)
        .ToListAsync();
  }

  public async Task<List<OrdemCompra>> ObterPorDataReferenciaAsync(DateTime dataReferencia)
  {
    var inicio = dataReferencia.Date;
    var fim = dataReferencia.Date.AddDays(1).AddTicks(-1);

    return await _context.OrdensCompra
        .Include(o => o.Distribuicoes)
        .Where(o => o.DataExecucao >= inicio && o.DataExecucao <= fim)
        .OrderByDescending(o => o.DataExecucao)
        .ToListAsync();
  }

  public async Task AddAsync(OrdemCompra ordem)
  {
    await _context.OrdensCompra.AddAsync(ordem);
  }

  public async Task UpdateAsync(OrdemCompra ordem)
  {
    _context.OrdensCompra.Update(ordem);
    await Task.CompletedTask;
  }

  public Task SaveChangesAsync()
  {
    return _context.SaveChangesAsync();
  }
}
