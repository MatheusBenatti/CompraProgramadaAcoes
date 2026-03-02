using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CompraProgramadaAcoes.Infrastructure.Repositories;

public class DistribuicaoRepository : IDistribuicaoRepository
{
    private readonly ApplicationDbContext _context;

    public DistribuicaoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Distribuicao?> ObterPorIdAsync(long id)
    {
        return await _context.Distribuicoes
            .Include(d => d.OrdemCompra)
            .Include(d => d.CustodiaFilhote)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<List<Distribuicao>> ObterPorOrdemCompraAsync(long ordemCompraId)
    {
        return await _context.Distribuicoes
            .Include(d => d.OrdemCompra)
            .Include(d => d.CustodiaFilhote)
            .Where(d => d.OrdemCompraId == ordemCompraId)
            .ToListAsync();
    }

    public async Task<List<Distribuicao>> ObterPorClienteAsync(long clienteId)
    {
        return await _context.Distribuicoes
            .Include(d => d.OrdemCompra)
            .Include(d => d.CustodiaFilhote)
            .Where(d => d.CustodiaFilhote.ContaGrafica.ClienteId == clienteId)
            .OrderByDescending(d => d.DataDistribuicao)
            .ToListAsync();
    }

    public async Task<List<Distribuicao>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim)
    {
        return await _context.Distribuicoes
            .Include(d => d.OrdemCompra)
            .Include(d => d.CustodiaFilhote)
            .Where(d => d.DataDistribuicao >= inicio && d.DataDistribuicao <= fim)
            .OrderByDescending(d => d.DataDistribuicao)
            .ToListAsync();
    }

    public async Task AddAsync(Distribuicao distribuicao)
    {
        await _context.Distribuicoes.AddAsync(distribuicao);
    }

    public async Task UpdateAsync(Distribuicao distribuicao)
    {
        _context.Distribuicoes.Update(distribuicao);
        await Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
