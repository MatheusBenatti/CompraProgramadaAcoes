using Microsoft.AspNetCore.Mvc;
using CompraProgramadaAcoes.Application.Interfaces;
using CompraProgramadaAcoes.Application.DTOs;

namespace CompraProgramadaAcoes.Api.Controllers;

[ApiController]
[Route("api/motor")]
public class MotorController : ControllerBase
{
  private readonly IMotorCompraProgramada _motorCompra;

  public MotorController(IMotorCompraProgramada motorCompra)
  {
    _motorCompra = motorCompra;
  }

  /// <summary>
  /// Executar compra manualmente (para testes)
  /// </summary>
  [HttpPost("executar-compra")]
  public async Task<ActionResult<ExecucaoCompraResponse>> ExecutarCompra([FromBody] ExecutarCompraRequest request)
  {
    try
    {
      var dataReferencia = DateTime.Parse(request.DataReferencia);
      await _motorCompra.ExecutarComprasProgramadasAsync(dataReferencia);

      // Simular resposta detalhada (em implementação real, buscar dados do banco)
      var response = new ExecucaoCompraResponse
      {
        DataExecucao = DateTime.UtcNow,
        TotalClientes = 3,
        TotalConsolidado = 3500.00m,
        OrdensCompra = new List<OrdemCompraResponse>
                {
                    new()
                    {
                        Ticker = "PETR4",
                        QuantidadeTotal = 28,
                        Detalhes = new List<OrdemCompraDetalheResponse>
                        {
                            new() { Tipo = "FRACIONARIO", Ticker = "PETR4F", Quantidade = 28 }
                        },
                        PrecoUnitario = 35.00m,
                        ValorTotal = 980.00m
                    },
                    new()
                    {
                        Ticker = "VALE3",
                        QuantidadeTotal = 14,
                        Detalhes = new List<OrdemCompraDetalheResponse>
                        {
                            new() { Tipo = "FRACIONARIO", Ticker = "VALE3F", Quantidade = 14 }
                        },
                        PrecoUnitario = 62.00m,
                        ValorTotal = 868.00m
                    }
                },
        Distribuicoes = new List<DistribuicaoResponse>
                {
                    new()
                    {
                        ClienteId = 1,
                        Nome = "Joao da Silva",
                        ValorAporte = 1000.00m,
                        Ativos = new List<AtivoDistribuidoResponse>
                        {
                            new() { Ticker = "PETR4", Quantidade = 8 },
                            new() { Ticker = "VALE3", Quantidade = 4 }
                        }
                    }
                },
        ResiduosCustMaster = new List<ResiduoResponse>
                {
                    new() { Ticker = "PETR4", Quantidade = 1 },
                    new() { Ticker = "ITUB4", Quantidade = 1 }
                },
        EventosIRPublicados = 15,
        Mensagem = "Compra programada executada com sucesso para 3 clientes."
      };

      return Ok(response);
    }
    catch (Exception ex)
    {
      return BadRequest(new ErrorResponse
      {
        Erro = ex.Message,
        Codigo = "ERRO_EXECUCAO_COMPRA"
      });
    }
  }
}

// DTOs para resposta do motor
public class ExecutarCompraRequest
{
  public string DataReferencia { get; set; } = string.Empty;
}

public class ExecucaoCompraResponse
{
  public DateTime DataExecucao { get; set; }
  public int TotalClientes { get; set; }
  public decimal TotalConsolidado { get; set; }
  public List<OrdemCompraResponse> OrdensCompra { get; set; } = new();
  public List<DistribuicaoResponse> Distribuicoes { get; set; } = new();
  public List<ResiduoResponse> ResiduosCustMaster { get; set; } = new();
  public int EventosIRPublicados { get; set; }
  public string Mensagem { get; set; } = string.Empty;
}

public class OrdemCompraResponse
{
  public string Ticker { get; set; } = string.Empty;
  public int QuantidadeTotal { get; set; }
  public List<OrdemCompraDetalheResponse> Detalhes { get; set; } = new();
  public decimal PrecoUnitario { get; set; }
  public decimal ValorTotal { get; set; }
}

public class OrdemCompraDetalheResponse
{
  public string Tipo { get; set; } = string.Empty;
  public string Ticker { get; set; } = string.Empty;
  public int Quantidade { get; set; }
}

public class DistribuicaoResponse
{
  public long ClienteId { get; set; }
  public string Nome { get; set; } = string.Empty;
  public decimal ValorAporte { get; set; }
  public List<AtivoDistribuidoResponse> Ativos { get; set; } = new();
}

public class AtivoDistribuidoResponse
{
  public string Ticker { get; set; } = string.Empty;
  public int Quantidade { get; set; }
}

public class ResiduoResponse
{
  public string Ticker { get; set; } = string.Empty;
  public int Quantidade { get; set; }
}
