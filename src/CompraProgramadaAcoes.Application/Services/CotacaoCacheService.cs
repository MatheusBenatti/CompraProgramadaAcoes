using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Application.Interfaces;
using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Application.Services;

public class CotacaoCacheService(ICacheService cacheService) : ICotacaoCacheService
{
  private readonly ICacheService _cacheService = cacheService;
  private const string COTACOES_KEY = "cotacoes:cesta";

  public async Task<CotacoesCestaDTO?> ObterCotacoesAsync()
  {
    return await _cacheService.GetAsync<CotacoesCestaDTO>(COTACOES_KEY);
  }

  public async Task SalvarCotacoesAsync(CotacoesCestaDTO cotacoes)
  {
    await _cacheService.SetAsync(COTACOES_KEY, cotacoes);
  }

  public async Task<CotacaoCacheDTO?> ObterCotacaoPorTickerAsync(string ticker)
  {
    var cotacoes = await ObterCotacoesAsync();
    return cotacoes?.ObterCotacao(ticker);
  }

  public async Task SalvarCotacoesDaCestaAsync(IEnumerable<Cotacao> cotacoes, IEnumerable<string> tickersCesta)
  {
    var tickersSet = tickersCesta.Select(t => t.ToUpper()).ToHashSet();
    var cotacoesFiltradas = cotacoes
        .Where(c => tickersSet.Contains(c.Ticker.ToUpper()))
        .ToList();

    if (cotacoesFiltradas.Count == 0)
      return;

    var cotacoesCesta = new CotacoesCestaDTO
    {
      DataReferencia = DateTime.UtcNow,
      Cotacoes = cotacoesFiltradas.ToDictionary(
            c => c.Ticker.ToUpper(),
            c => new CotacaoCacheDTO
            {
              Ticker = c.Ticker.ToUpper(),
              DataPregao = c.DataPregao,
              PrecoAbertura = c.PrecoAbertura,
              PrecoFechamento = c.PrecoFechamento,
              PrecoMaximo = c.PrecoMaximo,
              PrecoMinimo = c.PrecoMinimo,
              PrecoMedio = c.PrecoMedio,
              QuantidadeNegociada = c.QuantidadeNegociada,
              VolumeNegociado = c.VolumeNegociado,
              DataAtualizacao = DateTime.UtcNow
            }
        )
    };

    await SalvarCotacoesAsync(cotacoesCesta);
  }

  public async Task<Dictionary<string, decimal>> ObterPrecosFechamentoAsync(IEnumerable<string> tickers)
  {
    var cotacoes = await ObterCotacoesAsync();
    if (cotacoes == null)
      return [];

    var resultado = new Dictionary<string, decimal>();
    foreach (var ticker in tickers)
    {
      var cotacao = cotacoes.ObterCotacao(ticker);
      if (cotacao != null)
      {
        resultado[ticker.ToUpper()] = cotacao.PrecoFechamento;
      }
    }

    return resultado;
  }
}
