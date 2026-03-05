using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Application.Interfaces.Repositories;

public interface ICotacaoRepository
{
    Task BulkInsertAsync(IEnumerable<Cotacao> cotacoes);
    Task<IEnumerable<Cotacao>> ObterPorDataAsync(DateTime data);
    Task<Cotacao?> ObterPorTickerEDataAsync(string ticker, DateTime data);
    Task<IEnumerable<Cotacao>> ObterPorTickersEDataAsync(IEnumerable<string> tickers, DateTime data);
}
