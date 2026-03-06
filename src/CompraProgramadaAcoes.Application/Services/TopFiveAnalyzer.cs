using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Application.Services;

public class TopFiveAnalyzer
{
  /// Analisa as cotações e identifica o Top 5 ações com maior volume financeiro
  public CestaCacheDTO GerarCestaTopFive(IEnumerable<Cotacao> cotacoes)
  {
    // Filtrar apenas mercado a vista (010) e ordenar por volume financeiro
    var topCinco = cotacoes
        .Where(c => c.TipoMercado == 10) // Apenas mercado a vista
        .Where(c => c.VolumeNegociado > 0) // Com volume negociado
        .OrderByDescending(c => c.VolumeNegociado) // Maior volume primeiro
        .Take(5)
        .ToList();

    if (topCinco.Count < 5)
      throw new InvalidOperationException($"Apenas {topCinco.Count} ações encontradas com volume negociado. Mínimo necessário: 5");

    // Calcular volume total do Top 5
    var volumeTotal = topCinco.Sum(c => c.VolumeNegociado);

    // Criar cesta com percentuais baseados no volume de cada ação
    var cesta = new CestaCacheDTO
    {
      Nome = "Top Five",
      DataCriacao = DateTime.UtcNow,
      Ativa = true,
      Itens = [.. topCinco.Select(c => new ItemCestaCacheDTO
      {
        Ticker = c.Ticker.ToUpper(),
        Percentual = Math.Round((c.VolumeNegociado / volumeTotal) * 100, 2)
      })]
    };

    // Validar que a soma dos percentuais seja aproximadamente 100%
    var somaPercentuais = cesta.Itens.Sum(i => i.Percentual);
    if (Math.Abs(somaPercentuais - 100) > 0.01m)
    {
      // Ajustar para garantir soma = 100%
      var diferenca = 100 - somaPercentuais;
      var maiorPercentual = cesta.Itens.OrderByDescending(i => i.Percentual).First();
      maiorPercentual.Percentual += diferenca;
    }

    return cesta;
  }

  /// Obtém estatísticas do Top 5 para análise
  public static Dictionary<string, decimal> ObterEstatisticasTopFive(IEnumerable<Cotacao> cotacoes)
  {
    var topCinco = cotacoes
        .Where(c => c.TipoMercado == 10)
        .Where(c => c.VolumeNegociado > 0)
        .OrderByDescending(c => c.VolumeNegociado)
        .Take(5)
        .ToList();

    return topCinco.ToDictionary(
        c => c.Ticker.ToUpper(),
        c => c.VolumeNegociado
    );
  }

  /// Valida se a cesta atual ainda é relevante baseada no volume do dia
  public static bool CestaAindaRelevante(CestaCacheDTO cestaAtual, IEnumerable<Cotacao> cotacoesNovas)
  {
    var topCincoNovo = cotacoesNovas
        .Where(c => c.TipoMercado == 10)
        .Where(c => c.VolumeNegociado > 0)
        .OrderByDescending(c => c.VolumeNegociado)
        .Take(5)
        .Select(c => c.Ticker.ToUpper())
        .ToHashSet();

    var tickersCestaAtual = cestaAtual.Itens.Select(i => i.Ticker).ToHashSet();

    // Considera relevante se pelo menos 3 dos 5 tickers ainda estão no Top 5
    var tickersEmComum = tickersCestaAtual.Intersect(topCincoNovo).Count();
    return tickersEmComum >= 3;
  }
}
