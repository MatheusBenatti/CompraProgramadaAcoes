using CompraProgramadaAcoes.Application.DTOs.Admin;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;

namespace CompraProgramadaAcoes.Application.UseCases.Admin;

public class ObterHistoricoCestasUseCase(ICestaRecomendacaoRepository cestaRepository)
{
  private readonly ICestaRecomendacaoRepository _cestaRepository = cestaRepository;

  public async Task<CestasHistoricoResponse> ExecuteAsync()
  {
    var cestas = await _cestaRepository.ObterHistoricoAsync();

    return new CestasHistoricoResponse
    {
      Cestas = [.. cestas.Select(c => new CestaHistoricoItemResponse
            {
                CestaId = c.Id,
                Nome = c.Nome,
                Ativa = c.Ativa,
                DataCriacao = c.DataCriacao,
                DataDesativacao = c.DataDesativacao,
                Itens = [.. c.Itens.Select(i => new CestaItemAdminResponse
                {
                    Ticker = i.Ticker,
                    Percentual = i.Percentual
                })]
            })]
    };
  }
}
