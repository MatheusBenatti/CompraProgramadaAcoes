using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Application.Interfaces.Repositories;

public interface ICotacaoB3Repository
{
    Task BulkInsertAsync(IEnumerable<CotacaoB3> cotacoes);
    Task<IEnumerable<CotacaoB3>> ObterPorDataAsync(DateTime data);
    Task<CotacaoB3?> ObterPorTickerEDataAsync(string ticker, DateTime data);
    Task<IEnumerable<CotacaoB3>> ObterPorTickersEDataAsync(IEnumerable<string> tickers, DateTime data);
}
