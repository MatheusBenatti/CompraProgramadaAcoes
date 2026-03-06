using CompraProgramadaAcoes.Application.DTOs.Motor;
using CompraProgramadaAcoes.Application.Interfaces.Services;
using CompraProgramadaAcoes.Application.UseCases.Motor;

namespace CompraProgramadaAcoes.Application.Services;

public class MotorService(ExecutarCompraUseCase executarCompraUseCase) : IMotorService
{
  private readonly ExecutarCompraUseCase _executarCompraUseCase = executarCompraUseCase;

  public async Task<ExecucaoCompraResponse> ExecutarCompraAsync(ExecutarCompraRequest request)
  {
    return await _executarCompraUseCase.ExecuteAsync(request);
  }
}
