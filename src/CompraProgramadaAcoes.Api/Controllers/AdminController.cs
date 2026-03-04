using Microsoft.AspNetCore.Mvc;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Application.Interfaces;
using CompraProgramadaAcoes.Domain.Entities;
using CompraProgramadaAcoes.Application.Services;
using CompraProgramadaAcoes.Application.DTOs;

namespace CompraProgramadaAcoes.Api.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly ICestaRecomendacaoRepository _cestaRepository;
    private readonly IContaGraficaRepository _contaGraficaRepository;
    private readonly ICustodiaRepository _custodiaRepository;
    private readonly CotahistParser _cotahistParser;
    private readonly ICestaCacheService _cestaCacheService;

    public AdminController(
        ICestaRecomendacaoRepository cestaRepository,
        IContaGraficaRepository contaGraficaRepository,
        ICustodiaRepository custodiaRepository,
        CotahistParser cotahistParser,
        ICestaCacheService cestaCacheService)
    {
        _cestaRepository = cestaRepository;
        _contaGraficaRepository = contaGraficaRepository;
        _custodiaRepository = custodiaRepository;
        _cotahistParser = cotahistParser;
        _cestaCacheService = cestaCacheService;
    }

    /// <summary>
    /// Cadastra ou altera a cesta Top Five
    /// </summary>
    [HttpPost("cesta")]
    public async Task<ActionResult<CestaAdminResponse>> CadastrarCesta([FromBody] CadastrarCestaAdminRequest request)
    {
        try
        {
            // Inativar cesta vigente atual
            var cestaVigente = await _cestaRepository.ObterCestaVigenteAsync();
            bool rebalanceamentoDisparado = false;
            List<string> ativosRemovidos = new();
            List<string> ativosAdicionados = new();

            if (cestaVigente != null)
            {
                // Identificar ativos removidos e adicionados
                var tickersAntigos = cestaVigente.Itens.Select(i => i.Ticker).ToHashSet();
                var tickersNovos = request.Itens.Select(i => i.Ticker).ToHashSet();
                
                ativosRemovidos = tickersAntigos.Except(tickersNovos).ToList();
                ativosAdicionados = tickersNovos.Except(tickersAntigos).ToList();
                
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
                    return BadRequest(new ErrorResponse
                    {
                        Erro = $"A cesta deve conter exatamente 5 ativos. Quantidade informada: {novaCesta.Itens.Count}.",
                        Codigo = "QUANTIDADE_ATIVOS_INVALIDA"
                    });
                }
                
                return BadRequest(new ErrorResponse
                {
                    Erro = "A soma dos percentuais deve ser exatamente 100%.",
                    Codigo = "PERCENTUAIS_INVALIDOS"
                });
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

            var response = new CestaAdminResponse
            {
                CestaId = novaCesta.Id,
                Nome = novaCesta.Nome,
                Ativa = novaCesta.Ativa,
                DataCriacao = novaCesta.DataCriacao,
                Itens = novaCesta.Itens.Select(i => new CestaItemAdminResponse
                {
                    Ticker = i.Ticker,
                    Percentual = i.Percentual
                }).ToList(),
                RebalanceamentoDisparado = rebalanceamentoDisparado,
                Mensagem = rebalanceamentoDisparado 
                    ? $"Cesta atualizada. Rebalanceamento disparado para ativos removidos: [{string.Join(", ", ativosRemovidos)}] e adicionados: [{string.Join(", ", ativosAdicionados)}]."
                    : "Primeira cesta cadastrada com sucesso."
            };

            return CreatedAtAction(nameof(ObterCestaAtual), new { }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse
            {
                Erro = ex.Message,
                Codigo = "PERCENTUAIS_INVALIDOS"
            });
        }
    }

    /// <summary>
    /// Consulta cesta atual
    /// </summary>
    [HttpGet("cesta/atual")]
    public async Task<ActionResult<CestaAtualResponse>> ObterCestaAtual()
    {
        var cesta = await _cestaRepository.ObterCestaVigenteAsync();
        
        if (cesta == null)
            return NotFound(new ErrorResponse
            {
                Erro = "Nenhuma cesta ativa encontrada.",
                Codigo = "CESTA_NAO_ENCONTRADA"
            });

        // Obter cotações atuais
        var tickers = cesta.Itens.Select(i => i.Ticker);
        var cotacoes = _cotahistParser.ObterCotacoesFechamento("cotacoes", tickers);

        var response = new CestaAtualResponse
        {
            CestaId = cesta.Id,
            Nome = cesta.Nome,
            Ativa = cesta.Ativa,
            DataCriacao = cesta.DataCriacao,
            Itens = cesta.Itens.Select(i => new CestaItemAtualResponse
            {
                Ticker = i.Ticker,
                Percentual = i.Percentual,
                CotacaoAtual = cotacoes.GetValueOrDefault(i.Ticker)?.PrecoFechamento ?? 0
            }).ToList()
        };

        return Ok(response);
    }

    /// <summary>
    /// Histórico de cestas
    /// </summary>
    [HttpGet("cesta/historico")]
    public async Task<ActionResult<CestasHistoricoResponse>> ObterHistoricoCestas()
    {
        var cestas = await _cestaRepository.ObterHistoricoAsync();
        
        var response = new CestasHistoricoResponse
        {
            Cestas = cestas.Select(c => new CestaHistoricoItemResponse
            {
                CestaId = c.Id,
                Nome = c.Nome,
                Ativa = c.Ativa,
                DataCriacao = c.DataCriacao,
                DataDesativacao = c.DataDesativacao,
                Itens = c.Itens.Select(i => new CestaItemAdminResponse
                {
                    Ticker = i.Ticker,
                    Percentual = i.Percentual
                }).ToList()
            }).ToList()
        };

        return Ok(response);
    }

    /// <summary>
    /// Consultar custódia master
    /// </summary>
    [HttpGet("conta-master/custodia")]
    public async Task<ActionResult<ContaMasterCustodiaResponse>> ObterCustodiaMaster()
    {
        // Buscar conta master (tipo MASTER)
        var contas = await _contaGraficaRepository.ObterPorTipoAsync("MASTER");
        var contaMaster = contas.FirstOrDefault();
        
        if (contaMaster == null)
            return NotFound("Conta master não encontrada");

        var custodia = await _custodiaRepository.ObterPorContaGraficaAsync(contaMaster.Id);
        
        // Obter cotações atuais
        var tickers = custodia.Where(c => c.Quantidade > 0).Select(c => c.Ticker);
        var cotacoes = _cotahistParser.ObterCotacoesFechamento("cotacoes", tickers);

        var itensCustodia = custodia
            .Where(c => c.Quantidade > 0)
            .Select(c => new ContaMasterCustodiaItemResponse
            {
                Ticker = c.Ticker,
                Quantidade = c.Quantidade,
                PrecoMedio = c.PrecoMedio,
                ValorAtual = cotacoes.GetValueOrDefault(c.Ticker)?.PrecoFechamento ?? 0,
                Origem = "Residuo distribuicao"
            }).ToList();

        var valorTotalResiduo = itensCustodia.Sum(i => i.ValorAtual * i.Quantidade);

        var response = new ContaMasterCustodiaResponse
        {
            ContaMaster = new ContaMasterInfoResponse
            {
                Id = contaMaster.Id,
                NumeroConta = contaMaster.NumeroConta,
                Tipo = contaMaster.Tipo
            },
            Custodia = itensCustodia,
            ValorTotalResiduo = valorTotalResiduo
        };

        return Ok(response);
    }
}

// DTOs para respostas administrativas
public class CestaAdminResponse
{
    public long CestaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativa { get; set; }
    public DateTime DataCriacao { get; set; }
    public List<CestaItemAdminResponse> Itens { get; set; } = new();
    public bool RebalanceamentoDisparado { get; set; }
    public string Mensagem { get; set; } = string.Empty;
}

public class CestaItemAdminResponse
{
    public string Ticker { get; set; } = string.Empty;
    public decimal Percentual { get; set; }
}

public class CadastrarCestaAdminRequest
{
    public string Nome { get; set; } = string.Empty;
    public List<CadastrarCestaItemRequest> Itens { get; set; } = new();
}

public class CadastrarCestaItemRequest
{
    public string Ticker { get; set; } = string.Empty;
    public decimal Percentual { get; set; }
}

public class CestaAtualResponse
{
    public long CestaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativa { get; set; }
    public DateTime DataCriacao { get; set; }
    public List<CestaItemAtualResponse> Itens { get; set; } = new();
}

public class CestaItemAtualResponse
{
    public string Ticker { get; set; } = string.Empty;
    public decimal Percentual { get; set; }
    public decimal CotacaoAtual { get; set; }
}

public class CestasHistoricoResponse
{
    public List<CestaHistoricoItemResponse> Cestas { get; set; } = new();
}

public class CestaHistoricoItemResponse
{
    public long CestaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativa { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataDesativacao { get; set; }
    public List<CestaItemAdminResponse> Itens { get; set; } = new();
}

public class ContaMasterCustodiaResponse
{
    public ContaMasterInfoResponse ContaMaster { get; set; } = null!;
    public List<ContaMasterCustodiaItemResponse> Custodia { get; set; } = new();
    public decimal ValorTotalResiduo { get; set; }
}

public class ContaMasterInfoResponse
{
    public long Id { get; set; }
    public string NumeroConta { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
}

public class ContaMasterCustodiaItemResponse
{
    public string Ticker { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoMedio { get; set; }
    public decimal ValorAtual { get; set; }
    public string Origem { get; set; } = string.Empty;
}
