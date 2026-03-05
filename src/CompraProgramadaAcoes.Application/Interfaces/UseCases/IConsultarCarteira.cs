using CompraProgramadaAcoes.Application.DTOs;

namespace CompraProgramadaAcoes.Application.Interfaces.UseCases;

public interface IConsultarCarteira
{
    Task<CarteiraResponse> ExecuteAsync(long clienteId);
}
