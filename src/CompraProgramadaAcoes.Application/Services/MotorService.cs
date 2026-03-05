using CompraProgramadaAcoes.Application.DTOs.Motor;
using CompraProgramadaAcoes.Application.Interfaces.Services;
using CompraProgramadaAcoes.Application.UseCases.Motor;

namespace CompraProgramadaAcoes.Application.Services;

public class MotorService : IMotorService
{
    private readonly ExecutarCompraUseCase _executarCompraUseCase;

    public MotorService(ExecutarCompraUseCase executarCompraUseCase)
    {
        _executarCompraUseCase = executarCompraUseCase;
    }

    public async Task<ExecucaoCompraResponse> ExecutarCompraAsync(ExecutarCompraRequest request)
    {
        return await _executarCompraUseCase.ExecuteAsync(request);
    }
}
