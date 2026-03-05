using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Domain.Entities;
using CompraProgramadaAcoes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CompraProgramadaAcoes.Infrastructure.Repositories;

public class CotacaoRepository : ICotacaoRepository
{
    private readonly AppDbContext _context;

    public CotacaoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task BulkInsertAsync(IEnumerable<Cotacao> cotacoes)
    {
        if (cotacoes == null || !cotacoes.Any())
            return;

        // Limpar dados existentes para a mesma data para evitar duplicatas
        var dataPregao = cotacoes.First().DataPregao;
        var existentes = await _context.Cotacoes
            .Where(c => c.DataPregao.Date == dataPregao.Date)
            .ToListAsync();

        if (existentes.Any())
        {
            _context.Cotacoes.RemoveRange(existentes);
        }

        // Adicionar novas cotações
        await _context.Cotacoes.AddRangeAsync(cotacoes);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Cotacao>> ObterPorDataAsync(DateTime data)
    {
        return await _context.Cotacoes
            .Where(c => c.DataPregao.Date == data.Date)
            .OrderBy(c => c.Ticker)
            .ToListAsync();
    }

    public async Task<Cotacao?> ObterPorTickerEDataAsync(string ticker, DateTime data)
    {
        return await _context.Cotacoes
            .FirstOrDefaultAsync(c => c.Ticker == ticker && c.DataPregao.Date == data.Date);
    }

    public async Task<IEnumerable<Cotacao>> ObterPorTickersEDataAsync(IEnumerable<string> tickers, DateTime data)
    {
        return await _context.Cotacoes
            .Where(c => tickers.Contains(c.Ticker) && c.DataPregao.Date == data.Date)
            .OrderBy(c => c.Ticker)
            .ToListAsync();
    }
}
