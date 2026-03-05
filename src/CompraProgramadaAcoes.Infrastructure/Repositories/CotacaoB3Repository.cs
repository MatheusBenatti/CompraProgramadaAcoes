using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Domain.Entities;
using CompraProgramadaAcoes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CompraProgramadaAcoes.Infrastructure.Repositories;

public class CotacaoB3Repository : ICotacaoB3Repository
{
    private readonly AppDbContext _context;

    public CotacaoB3Repository(AppDbContext context)
    {
        _context = context;
    }

    public async Task BulkInsertAsync(IEnumerable<CotacaoB3> cotacoes)
    {
        if (cotacoes == null || !cotacoes.Any())
            return;

        // Limpar dados existentes para a mesma data para evitar duplicatas
        var dataPregao = cotacoes.First().DataPregao;
        var existentes = await _context.CotacoesB3
            .Where(c => c.DataPregao.Date == dataPregao.Date)
            .ToListAsync();

        if (existentes.Any())
        {
            _context.CotacoesB3.RemoveRange(existentes);
        }

        // Adicionar novas cotações
        await _context.CotacoesB3.AddRangeAsync(cotacoes);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<CotacaoB3>> ObterPorDataAsync(DateTime data)
    {
        return await _context.CotacoesB3
            .Where(c => c.DataPregao.Date == data.Date)
            .OrderBy(c => c.Ticker)
            .ToListAsync();
    }

    public async Task<CotacaoB3?> ObterPorTickerEDataAsync(string ticker, DateTime data)
    {
        return await _context.CotacoesB3
            .FirstOrDefaultAsync(c => c.Ticker == ticker && c.DataPregao.Date == data.Date);
    }

    public async Task<IEnumerable<CotacaoB3>> ObterPorTickersEDataAsync(IEnumerable<string> tickers, DateTime data)
    {
        return await _context.CotacoesB3
            .Where(c => tickers.Contains(c.Ticker) && c.DataPregao.Date == data.Date)
            .OrderBy(c => c.Ticker)
            .ToListAsync();
    }
}
