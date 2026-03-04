using CompraProgramadaAcoes.Application.DTOs;

namespace CompraProgramadaAcoes.Application.Interfaces.UseCases;

public interface IRealizarAdesao
{
    Task<AdesaoResponse> ExecuteAsync(AdesaoRequest request);
}
