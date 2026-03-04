using CompraProgramadaAcoes.Application.DTOs;

namespace CompraProgramadaAcoes.Application.Interfaces.UseCases;

public interface IRealizarSaida
{
    Task<SaidaResponse> ExecuteAsync(int clienteId);
}
