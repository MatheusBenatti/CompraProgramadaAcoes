using CompraProgramadaAcoes.Application.DTOs.Admin;

namespace CompraProgramadaAcoes.Application.Interfaces.Services;

public interface IAdminService
{
    Task<CestaAdminResponse> CadastrarCestaAsync(CadastrarCestaAdminRequest request);
    Task<CestaAtualResponse> ObterCestaAtualAsync();
    Task<CestasHistoricoResponse> ObterHistoricoCestasAsync();
    Task<ContaMasterCustodiaResponse> ObterCustodiaMasterAsync();
}
