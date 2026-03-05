using CompraProgramadaAcoes.Application.DTOs.Rebalanceamento;
using CompraProgramadaAcoes.Application.Interfaces.Services;
using CompraProgramadaAcoes.Application.UseCases.Rebalanceamento;

namespace CompraProgramadaAcoes.Application.Services;

public class RebalanceamentoService : IRebalanceamentoService
{
    private readonly RebalancearPorMudancaCestaUseCase _rebalancearPorMudancaCestaUseCase;

    public RebalanceamentoService(RebalancearPorMudancaCestaUseCase rebalancearPorMudancaCestaUseCase)
    {
        _rebalancearPorMudancaCestaUseCase = rebalancearPorMudancaCestaUseCase;
    }

    public async Task<RebalanceamentoResponse> RebalancearPorMudancaCestaAsync(RebalancearMudancaCestaRequest request)
    {
        return await _rebalancearPorMudancaCestaUseCase.ExecuteAsync(request);
    }
}
