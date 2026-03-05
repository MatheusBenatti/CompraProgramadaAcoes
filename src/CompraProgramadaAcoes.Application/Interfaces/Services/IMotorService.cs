using CompraProgramadaAcoes.Application.DTOs.Motor;

namespace CompraProgramadaAcoes.Application.Interfaces.Services;

public interface IMotorService
{
    Task<ExecucaoCompraResponse> ExecutarCompraAsync(ExecutarCompraRequest request);
}
