using CompraProgramadaAcoes.Application.DTOs;

namespace CompraProgramadaAcoes.Application.Interfaces.UseCases;

public interface IConsultarRentabilidade
{
    Task<RentabilidadeResponse> ExecuteAsync(long clienteId);
}
