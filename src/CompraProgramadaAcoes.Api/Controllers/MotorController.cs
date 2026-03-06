using Microsoft.AspNetCore.Mvc;
using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Application.DTOs.Motor;
using CompraProgramadaAcoes.Application.Interfaces.Services;
using CompraProgramadaAcoes.Application.Exceptions;

namespace CompraProgramadaAcoes.Api.Controllers;

/// <summary>
/// Controller para operações do motor de compras programadas
/// </summary>
[ApiController]
[Route("api/motor")]
public class MotorController(IMotorService motorService) : ControllerBase
{
  private readonly IMotorService _motorService = motorService;

  /// <summary>
  /// Executar compra manualmente (para testes)
  /// </summary>
  /// <param name="request">Dados da execução da compra</param>
  /// <returns>Resultado da execução da compra</returns>
  /// <response code="200">Compra executada com sucesso</response>
  /// <response code="400">Erro de negócio ou na execução</response>
  [HttpPost("executar-compra")]
  [ProducesResponseType(typeof(ExecucaoCompraResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
  public async Task<ActionResult<ExecucaoCompraResponse>> ExecutarCompra([FromBody] ExecutarCompraRequest request)
  {
    try
    {
      var result = await _motorService.ExecutarCompraAsync(request);
      return Ok(result);
    }
    catch (BusinessException ex)
    {
      return BadRequest(new ErrorResponse
      {
        Erro = ex.Message,
        Codigo = ex.ErrorCode
      });
    }
    catch (Exception)
    {
      return BadRequest(new ErrorResponse
      {
        Erro = "Erro ao executar compra programada.",
        Codigo = "ERRO_EXECUCAO_COMPRA"
      });
    }
  }
}
