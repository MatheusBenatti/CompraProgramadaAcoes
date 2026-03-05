using CompraProgramadaAcoes.Application.DTOs.Rebalanceamento;

namespace CompraProgramadaAcoes.Application.Interfaces.Services;

public interface IRebalanceamentoService
{
    Task<RebalanceamentoResponse> RebalancearPorMudancaCestaAsync(RebalancearMudancaCestaRequest request);
}
