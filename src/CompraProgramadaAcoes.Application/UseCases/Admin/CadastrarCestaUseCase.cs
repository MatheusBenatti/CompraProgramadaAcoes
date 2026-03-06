using CompraProgramadaAcoes.Application.DTOs.Admin;
using CompraProgramadaAcoes.Application.Interfaces;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Domain.Entities;
using CompraProgramadaAcoes.Application.Exceptions;
using CompraProgramadaAcoes.Application.DTOs;

namespace CompraProgramadaAcoes.Application.UseCases.Admin;

public class CadastrarCestaUseCase(
    ICestaRecomendacaoRepository cestaRepository,
    ICestaCacheService cestaCacheService)
{
  private readonly ICestaRecomendacaoRepository _cestaRepository = cestaRepository;
  private readonly ICestaCacheService _cestaCacheService = cestaCacheService;

  public async Task<CestaAdminResponse> ExecuteAsync(CadastrarCestaAdminRequest request)
  {
    // Inativar cesta vigente atual
    var cestaVigente = await _cestaRepository.ObterCestaVigenteAsync();
    bool rebalanceamentoDisparado = false;
    List<string> ativosRemovidos = [];
    List<string> ativosAdicionados = [];

    if (cestaVigente != null)
    {
      // Identificar ativos removidos e adicionados
      var tickersAntigos = cestaVigente.Itens.Select(i => i.Ticker).ToHashSet();
      var tickersNovos = request.Itens.Select(i => i.Ticker).ToHashSet();

      ativosRemovidos = [.. tickersAntigos.Except(tickersNovos)];
      ativosAdicionados = [.. tickersNovos.Except(tickersAntigos)];

      cestaVigente.Desativar();
      await _cestaRepository.UpdateAsync(cestaVigente);
      rebalanceamentoDisparado = true;
    }

    // Criar nova cesta
    var novaCesta = new CestaRecomendacao();
    novaCesta.AtualizarNome(request.Nome);

    foreach (var item in request.Itens)
    {
      novaCesta.AdicionarItem(item.Ticker, item.Percentual);
    }

    if (!novaCesta.IsValida())
    {
      if (novaCesta.Itens.Count != 5)
      {
        throw new BusinessException($"A cesta deve conter exatamente 5 ativos. Quantidade informada: {novaCesta.Itens.Count}.", "QUANTIDADE_ATIVOS_INVALIDA");
      }

      throw new BusinessException("A soma dos percentuais deve ser exatamente 100%.", "PERCENTUAIS_INVALIDOS");
    }

    await _cestaRepository.AddAsync(novaCesta);
    await _cestaRepository.SaveChangesAsync();

    // Converter cesta para DTO e salvar no cache
    var cestaDTO = new CestaCacheDTO
    {
      Nome = novaCesta.Nome,
      DataCriacao = novaCesta.DataCriacao,
      Ativa = novaCesta.Ativa,
      Itens = novaCesta.Itens.Select(i => new ItemCestaCacheDTO
      {
        Ticker = i.Ticker,
        Percentual = i.Percentual
      }).ToList()
    };
    await _cestaCacheService.SalvarCestaAsync(cestaDTO);

    return new CestaAdminResponse
    {
      CestaId = novaCesta.Id,
      Nome = novaCesta.Nome,
      Ativa = novaCesta.Ativa,
      DataCriacao = novaCesta.DataCriacao,
      Itens = [.. novaCesta.Itens.Select(i => new CestaItemAdminResponse
      {
        Ticker = i.Ticker,
        Percentual = i.Percentual
      })],
      RebalanceamentoDisparado = rebalanceamentoDisparado,
      Mensagem = rebalanceamentoDisparado
              ? $"Cesta atualizada. Rebalanceamento disparado para ativos removidos: [{string.Join(", ", ativosRemovidos)}] e adicionados: [{string.Join(", ", ativosAdicionados)}]."
              : "Primeira cesta cadastrada com sucesso."
    };
  }
}
