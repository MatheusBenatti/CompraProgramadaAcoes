using CompraProgramadaAcoes.Application.DTOs;

namespace CompraProgramadaAcoes.Application.Interfaces.UseCases;

public interface IAlterarValorMensal
{
    Task<AlterarValorMensalResponse> ExecuteAsync(int clienteId, AlterarValorMensalRequest request);
}
