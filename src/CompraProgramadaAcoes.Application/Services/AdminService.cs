using CompraProgramadaAcoes.Application.DTOs.Admin;
using CompraProgramadaAcoes.Application.Interfaces.Services;
using CompraProgramadaAcoes.Application.UseCases.Admin;

namespace CompraProgramadaAcoes.Application.Services;

public class AdminService : IAdminService
{
    private readonly CadastrarCestaUseCase _cadastrarCestaUseCase;
    private readonly ObterCestaAtualUseCase _obterCestaAtualUseCase;
    private readonly ObterHistoricoCestasUseCase _obterHistoricoCestasUseCase;
    private readonly ObterCustodiaMasterUseCase _obterCustodiaMasterUseCase;

    public AdminService(
        CadastrarCestaUseCase cadastrarCestaUseCase,
        ObterCestaAtualUseCase obterCestaAtualUseCase,
        ObterHistoricoCestasUseCase obterHistoricoCestasUseCase,
        ObterCustodiaMasterUseCase obterCustodiaMasterUseCase)
    {
        _cadastrarCestaUseCase = cadastrarCestaUseCase;
        _obterCestaAtualUseCase = obterCestaAtualUseCase;
        _obterHistoricoCestasUseCase = obterHistoricoCestasUseCase;
        _obterCustodiaMasterUseCase = obterCustodiaMasterUseCase;
    }

    public async Task<CestaAdminResponse> CadastrarCestaAsync(CadastrarCestaAdminRequest request)
    {
        return await _cadastrarCestaUseCase.ExecuteAsync(request);
    }

    public async Task<CestaAtualResponse> ObterCestaAtualAsync()
    {
        return await _obterCestaAtualUseCase.ExecuteAsync();
    }

    public async Task<CestasHistoricoResponse> ObterHistoricoCestasAsync()
    {
        return await _obterHistoricoCestasUseCase.ExecuteAsync();
    }

    public async Task<ContaMasterCustodiaResponse> ObterCustodiaMasterAsync()
    {
        return await _obterCustodiaMasterUseCase.ExecuteAsync();
    }
}
