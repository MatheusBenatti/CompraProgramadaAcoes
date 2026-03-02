using CompraProgramadaAcoes.Domain.ValueObjects;

namespace CompraProgramadaAcoes.Domain.Services;

public interface ICotacaoService
{
    Task<decimal> ObterCotacaoAtualAsync(Ticker ticker);
    Task<Dictionary<Ticker, decimal>> ObterCotacoesAsync(IEnumerable<Ticker> tickers);
    Task<bool> ExisteCotacaoAsync(Ticker ticker, DateTime data);
}
