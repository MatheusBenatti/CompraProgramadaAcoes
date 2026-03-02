using CompraProgramadaAcoes.Domain.Services;
using CompraProgramadaAcoes.Domain.ValueObjects;
using CompraProgramadaAcoes.Application.Services;

namespace CompraProgramadaAcoes.Infrastructure.Services;

public class CotacaoService : ICotacaoService
{
    private readonly CotahistParser _cotahistParser;
    private readonly string _pastaCotacoes;

    public CotacaoService(CotahistParser cotahistParser, string pastaCotacoes = "cotacoes")
    {
        _cotahistParser = cotahistParser;
        _pastaCotacoes = pastaCotacoes;
    }

    public Task<decimal> ObterCotacaoAtualAsync(Ticker ticker)
    {
        var cotacao = _cotahistParser.ObterCotacaoFechamento(_pastaCotacoes, ticker);
        return Task.FromResult(cotacao?.PrecoFechamento ?? 0);
    }

    public async Task<Dictionary<Ticker, decimal>> ObterCotacoesAsync(IEnumerable<Ticker> tickers)
    {
        var resultado = new Dictionary<Ticker, decimal>();
        
        foreach (var ticker in tickers)
        {
            var cotacao = await ObterCotacaoAtualAsync(ticker);
            resultado[ticker] = cotacao;
        }

        return resultado;
    }

    public Task<bool> ExisteCotacaoAsync(Ticker ticker, DateTime data)
    {
        // Implementação simplificada - verificar se existe arquivo para a data
        var nomeArquivo = $"COTAHIST_D{data:yyyyMMdd}.TXT";
        var caminhoArquivo = Path.Combine(_pastaCotacoes, nomeArquivo);
        
        if (!File.Exists(caminhoArquivo))
            return Task.FromResult(false);

        var cotacoes = _cotahistParser.ParseArquivo(caminhoArquivo);
        return Task.FromResult(cotacoes.Any(c => c.Ticker == ticker));
    }
}
