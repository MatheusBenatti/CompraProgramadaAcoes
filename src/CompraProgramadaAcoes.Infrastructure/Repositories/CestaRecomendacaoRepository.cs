using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Domain.Entities;
using CompraProgramadaAcoes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CompraProgramadaAcoes.Infrastructure.Repositories;

public class CestaRecomendacaoRepository(AppDbContext context) : ICestaRecomendacaoRepository
{
    private readonly AppDbContext _context = context;

  public async Task<CestaRecomendacao?> ObterCestaVigenteAsync()
    {
        return await _context.CestasRecomendacao
            .Include(c => c.Itens)
            .Where(c => c.Ativa)
            .OrderByDescending(c => c.DataCriacao)
            .FirstOrDefaultAsync();
    }

    public async Task<List<CestaRecomendacao>> ObterHistoricoAsync()
    {
        return await _context.CestasRecomendacao
            .Include(c => c.Itens)
            .OrderByDescending(c => c.DataCriacao)
            .ToListAsync();
    }

    public async Task<CestaRecomendacao?> ObterPorIdAsync(long id)
    {
        return await _context.CestasRecomendacao
            .Include(c => c.Itens)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(CestaRecomendacao cesta)
    {
        await _context.CestasRecomendacao.AddAsync(cesta);
    }

    public async Task UpdateAsync(CestaRecomendacao cesta)
    {
        _context.CestasRecomendacao.Update(cesta);
        await Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
