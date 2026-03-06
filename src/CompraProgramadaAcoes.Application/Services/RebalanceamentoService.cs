using CompraProgramadaAcoes.Application.DTOs.Rebalanceamento;
using CompraProgramadaAcoes.Application.Interfaces.Services;
using CompraProgramadaAcoes.Application.UseCases.Rebalanceamento;

namespace CompraProgramadaAcoes.Application.Services;

public class RebalanceamentoService(RebalancearPorMudancaCestaUseCase rebalancearPorMudancaCestaUseCase) : IRebalanceamentoService
{
  private readonly RebalancearPorMudancaCestaUseCase _rebalancearPorMudancaCestaUseCase = rebalancearPorMudancaCestaUseCase;

  public async Task<RebalanceamentoResponse> RebalancearPorMudancaCestaAsync(RebalancearMudancaCestaRequest request)
  {
    return await _rebalancearPorMudancaCestaUseCase.ExecuteAsync(request);
  }
}
