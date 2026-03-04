using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CompraProgramadaAcoes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CestaController : ControllerBase
{
    private readonly CestaCacheService _cestaCacheService;
    private readonly CotacaoCacheService _cotacaoCacheService;

    public CestaController(CestaCacheService cestaCacheService, CotacaoCacheService cotacaoCacheService)
    {
        _cestaCacheService = cestaCacheService;
        _cotacaoCacheService = cotacaoCacheService;
    }

    /// <summary>
    /// Obtém a cesta Top Five atual do Redis
    /// </summary>
    [HttpGet("top-five")]
    [ProducesResponseType(typeof(CestaCacheDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterCestaTopFive()
    {
        var cesta = await _cestaCacheService.ObterCestaAsync();
        
        if (cesta == null)
            return NotFound(new { Mensagem = "Cesta não encontrada no Redis" });

        return Ok(cesta);
    }

    /// <summary>
    /// Força a inicialização da cesta padrão no Redis
    /// </summary>
    [HttpPost("top-five/inicializar")]
    [ProducesResponseType(typeof(CestaCacheDTO), StatusCodes.Status200OK)]
    public async Task<IActionResult> InicializarCestaPadrao()
    {
        await _cestaCacheService.InicializarCestaPadraoAsync();
        var cesta = await _cestaCacheService.ObterCestaAsync();
        
        return Ok(new { 
            Mensagem = "Cesta inicializada com sucesso",
            Cesta = cesta
        });
    }

    /// <summary>
    /// Gera cesta Top Five baseada no arquivo COTAHIST
    /// </summary>
    [HttpPost("top-five/gerar-do-arquivo")]
    [ProducesResponseType(typeof(CestaCacheDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GerarCestaDoArquivo([FromQuery] string arquivoCotacoes)
    {
        try
        {
            var cotahistParser = new CotahistParser(_cestaCacheService, _cotacaoCacheService);
            var cotacoes = await cotahistParser.ParseArquivoAsync(arquivoCotacoes);
            
            var cesta = await _cestaCacheService.ObterCestaAsync();
            
            return Ok(new { 
                Mensagem = "Cesta gerada com sucesso baseada no volume do dia",
                TotalCotacoesProcessadas = cotacoes.Count(),
                Cesta = cesta
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { 
                Erro = ex.Message,
                Codigo = "ERRO_PROCESSAMENTO_ARQUIVO"
            });
        }
    }

    /// <summary>
    /// Salva uma cesta personalizada no Redis
    /// </summary>
    [HttpPost("top-five")]
    [ProducesResponseType(typeof(CestaCacheDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SalvarCesta([FromBody] CestaCacheDTO cesta)
    {
        try
        {
            await _cestaCacheService.SalvarCestaAsync(cesta);
            return Ok(new { 
                Mensagem = "Cesta salva com sucesso",
                Cesta = cesta
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { 
                Erro = ex.Message,
                Codigo = "CESTA_INVALIDA"
            });
        }
    }

    /// <summary>
    /// Obtém as cotações dos tickers da cesta do Redis
    /// </summary>
    [HttpGet("top-five/cotacoes")]
    [ProducesResponseType(typeof(CotacoesCestaDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterCotacoesCesta()
    {
        var cotacoes = await _cotacaoCacheService.ObterCotacoesAsync();
        
        if (cotacoes == null)
            return NotFound(new { Mensagem = "Cotações não encontradas no Redis" });

        return Ok(cotacoes);
    }

    /// <summary>
    /// Obtém cotação de um ticker específico da cesta
    /// </summary>
    [HttpGet("top-five/cotacoes/{ticker}")]
    [ProducesResponseType(typeof(CotacaoCacheDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterCotacaoPorTicker(string ticker)
    {
        var cotacao = await _cotacaoCacheService.ObterCotacaoPorTickerAsync(ticker);
        
        if (cotacao == null)
            return NotFound(new { Mensagem = $"Cotação do ticker {ticker} não encontrada" });

        return Ok(cotacao);
    }

    /// <summary>
    /// Obtém preços de fechamento dos tickers da cesta
    /// </summary>
    [HttpGet("top-five/precos")]
    [ProducesResponseType(typeof(Dictionary<string, decimal>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterPrecosFechamento()
    {
        var cesta = await _cestaCacheService.ObterCestaAsync();
        if (cesta?.Itens == null)
            return NotFound(new { Mensagem = "Cesta não encontrada" });

        var tickers = cesta.Itens.Select(i => i.Ticker);
        var precos = await _cotacaoCacheService.ObterPrecosFechamentoAsync(tickers);

        return Ok(new {
            DataReferencia = DateTime.UtcNow,
            Precos = precos
        });
    }
}
