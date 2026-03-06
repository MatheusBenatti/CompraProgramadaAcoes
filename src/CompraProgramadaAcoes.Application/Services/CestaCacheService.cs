using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Application.Interfaces;
using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Application.Services;

public class CestaCacheService(ICacheService cacheService) : ICestaCacheService
{
  private readonly ICacheService _cacheService = cacheService;
  private const string CESTA_KEY = "cesta:top_five";

  public async Task<CestaCacheDTO?> ObterCestaAsync()
  {
    return await _cacheService.GetAsync<CestaCacheDTO>(CESTA_KEY);
  }

  public async Task SalvarCestaAsync(CestaCacheDTO cesta)
  {
    if (!cesta.IsValida())
      throw new InvalidOperationException("Cesta inválida: deve ter exatamente 5 itens com soma 100%");

    await _cacheService.SetAsync(CESTA_KEY, cesta);
  }

  /// Gera e salva cesta Top Five baseada no volume do dia
  public async Task<CestaCacheDTO> GerarCestaDoDiaAsync(IEnumerable<Cotacao> cotacoes)
  {
    var analyzer = new TopFiveAnalyzer();
    var novaCesta = analyzer.GerarCestaTopFive(cotacoes);

    await SalvarCestaAsync(novaCesta);
    return novaCesta;
  }

  /// Atualiza cesta apenas se houver mudança significativa no Top 5
  public async Task<bool> AtualizarCestaSeNecessarioAsync(IEnumerable<Cotacao> cotacoes)
  {
    var cestaAtual = await ObterCestaAsync();
    var analyzer = new TopFiveAnalyzer();

    // Se não existe cesta, cria uma nova
    if (cestaAtual == null)
    {
      await GerarCestaDoDiaAsync(cotacoes);
      return true;
    }

    // Verifica se cesta atual ainda é relevante
    if (TopFiveAnalyzer.CestaAindaRelevante(cestaAtual, cotacoes))
    {
      return false; // Não precisa atualizar
    }

    // Gera nova cesta com base no volume do dia
    await GerarCestaDoDiaAsync(cotacoes);
    return true;
  }
}
