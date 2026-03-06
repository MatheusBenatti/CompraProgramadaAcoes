using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Application.Interfaces;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Domain.Entities;
using System.Text;
using System.Globalization;

namespace CompraProgramadaAcoes.Application.Services;

public class CotahistParser(ICestaCacheService cestaCacheService, ICotacaoCacheService cotacaoCacheService, ICotacaoRepository cotacaoRepository)
{
  private readonly ICestaCacheService _cestaCacheService = cestaCacheService;
  private readonly ICotacaoCacheService _cotacaoCacheService = cotacaoCacheService;
  private readonly ICotacaoRepository _cotacaoRepository = cotacaoRepository;

  /// <summary>
  /// Lê e faz parse de um arquivo COTAHIST da B3.
  /// Processa todos os registros de detalhe (TIPREG = 01).
  /// Aplica pré-processamento: filtra por mercado a vista (010) e fracionário (020),
  /// ordena por volume negociado e limita aos 100 mais relevantes.
  /// Salva apenas dados essenciais no banco, mas mantém dados completos
  /// para análises (Top Five, cache, etc.).
  /// </summary>
  public virtual async Task<IEnumerable<Cotacao>> ParseArquivoAsync(string caminhoArquivo)
  {
    var cotacoesCompletas = new List<Cotacao>();

    // Registrar encoding ISO-8859-1 para ler o arquivo corretamente
    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    var encoding = Encoding.GetEncoding("ISO-8859-1");

    foreach (var linha in File.ReadLines(caminhoArquivo, encoding))
    {
      // Ignorar header (00) e trailer (99)
      if (linha.Length < 245)
        continue;

      var tipoRegistro = linha.Substring(0, 2);
      if (tipoRegistro != "01")
        continue;

      var tipoMercadoStr = linha.Substring(24, 3).Trim();
      if (!int.TryParse(tipoMercadoStr, out var tipoMercado))
        continue;

      // Filtrar apenas mercado a vista (010) e fracionário (020)
      if (tipoMercado != 10 && tipoMercado != 20)
        continue;

      var cotacaoCompleta = new Cotacao
      {
        DataPregao = DateTime.ParseExact(
              linha.Substring(2, 8), "yyyyMMdd",
              CultureInfo.InvariantCulture),
        CodigoBDI = linha.Substring(10, 2).Trim(),
        Ticker = linha.Substring(12, 12).Trim(),
        TipoMercado = tipoMercado,
        NomeEmpresa = linha.Substring(27, 12).Trim(),
        PrecoAbertura = ParsePreco(linha.Substring(56, 13)),
        PrecoMaximo = ParsePreco(linha.Substring(69, 13)),
        PrecoMinimo = ParsePreco(linha.Substring(82, 13)),
        PrecoMedio = ParsePreco(linha.Substring(95, 13)),
        PrecoFechamento = ParsePreco(linha.Substring(108, 13)),
        QuantidadeNegociada = long.Parse(linha.Substring(152, 18).Trim()),
        VolumeNegociado = ParsePreco(linha.Substring(170, 18))
      };

      cotacoesCompletas.Add(cotacaoCompleta);
    }

    // Pré-processamento: filtrar por volume e mercado antes de salvar
    var cotacoesFiltradas = cotacoesCompletas
        .Where(c => c.TipoMercado == 10) // Apenas mercado a vista
        .Where(c => c.VolumeNegociado > 0) // Com volume negociado
        .OrderByDescending(c => c.VolumeNegociado) // Maior volume primeiro
        .Take(100) // Limitar aos 100 mais negociados para otimizar
        .ToList();

    // Converter para entidades simplificadas para salvar no banco
    var cotacoesParaSalvar = cotacoesFiltradas.Select(c => new Cotacao
    {
      DataPregao = c.DataPregao,
      Ticker = c.Ticker,
      PrecoAbertura = c.PrecoAbertura,
      PrecoFechamento = c.PrecoFechamento,
      PrecoMaximo = c.PrecoMaximo,
      PrecoMinimo = c.PrecoMinimo
    }).ToList();

    // Gerar cesta Top Five baseada no volume do dia (usando dados completos)
    var cestaFoiAtualizada = await _cestaCacheService.GerarCestaDoDiaAsync(cotacoesCompletas);

    // Salvar apenas dados essenciais no banco de dados
    if (cotacoesParaSalvar.Count != 0)
    {
      await _cotacaoRepository.BulkInsertAsync(cotacoesParaSalvar);
    }

    // Salvar cotações dos tickers da cesta no Redis para consultas rápidas (usando dados completos)
    var cesta = await _cestaCacheService.ObterCestaAsync();
    if (cesta?.Itens != null)
    {
      var tickersCesta = cesta.Itens.Select(i => i.Ticker);
      await _cotacaoCacheService.SalvarCotacoesDaCestaAsync(cotacoesCompletas, tickersCesta);
    }

    return cotacoesCompletas; // Retorna dados completos para análises
  }

  // Versão síncrona para compatibilidade
  public virtual IEnumerable<Cotacao> ParseArquivo(string caminhoArquivo)
  {
    return ParseArquivoAsync(caminhoArquivo).GetAwaiter().GetResult();
  }

  // Converte o valor inteiro do arquivo para decimal com 2 casas.
  // Ex: "0000000003850" => 38.50m
  private decimal ParsePreco(string valorBruto)
  {
    if (long.TryParse(valorBruto.Trim(), out var valor))
      return valor / 100m;
    return 0m;
  }

  // Obtém a cotação de fechamento mais recente de um ticker específico.
  // Busca na pasta cotacoes/ o arquivo mais recente.
  public virtual Cotacao? ObterCotacaoFechamento(string pastaCotacoes, string ticker)
  {
    var arquivos = Directory.GetFiles(pastaCotacoes, "COTAHIST_D*.TXT")
        .OrderByDescending(f => f)
        .ToList();

    foreach (var arquivo in arquivos)
    {
      var cotacoes = ParseArquivoAsync(arquivo).GetAwaiter().GetResult();
      var cotacao = cotacoes
          .Where(c => c.Ticker.Equals(ticker, StringComparison.OrdinalIgnoreCase))
          .FirstOrDefault();

      if (cotacao != null)
        return cotacao;
    }

    return null;
  }

  // Obtém cotações de fechamento para múltiplos tickers.
  public virtual Dictionary<string, Cotacao> ObterCotacoesFechamento(string pastaCotacoes, IEnumerable<string> tickers)
  {
    var resultado = new Dictionary<string, Cotacao>();

    foreach (var ticker in tickers)
    {
      var cotacao = ObterCotacaoFechamento(pastaCotacoes, ticker);
      if (cotacao != null)
        resultado[ticker] = cotacao;
    }

    return resultado;
  }
}
