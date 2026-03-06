using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Application.Interfaces;

public interface ICotacaoCacheService
{
  Task<CotacoesCestaDTO?> ObterCotacoesAsync();
  Task SalvarCotacoesAsync(CotacoesCestaDTO cotacoes);
  Task<CotacaoCacheDTO?> ObterCotacaoPorTickerAsync(string ticker);
  Task SalvarCotacoesDaCestaAsync(IEnumerable<Cotacao> cotacoes, IEnumerable<string> tickersCesta);
  Task<Dictionary<string, decimal>> ObterPrecosFechamentoAsync(IEnumerable<string> tickers);
}
