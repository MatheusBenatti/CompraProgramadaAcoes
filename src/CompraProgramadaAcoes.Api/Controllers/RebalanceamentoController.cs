using Microsoft.AspNetCore.Mvc;
using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Application.DTOs.Rebalanceamento;
using CompraProgramadaAcoes.Application.Interfaces.Services;
using CompraProgramadaAcoes.Application.Exceptions;

namespace CompraProgramadaAcoes.Api.Controllers;

/// <summary>
/// Controller para operações de rebalanceamento de carteiras
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RebalanceamentoController(IRebalanceamentoService rebalanceamentoService) : ControllerBase
{
  private readonly IRebalanceamentoService _rebalanceamentoService = rebalanceamentoService;

  /// <summary>
  /// Rebalanceia carteiras por mudança de cesta
  /// </summary>
  /// <param name="request">Dados do rebalanceamento por mudança de cesta</param>
  /// <returns>Resultado do rebalanceamento</returns>
  /// <response code="200">Rebalanceamento executado com sucesso</response>
  /// <response code="400">Erro de negócio</response>
  /// <response code="404">Recurso não encontrado</response>
  [HttpPost("mudanca-cesta")]
  [ProducesResponseType(typeof(RebalanceamentoResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
  public async Task<ActionResult<RebalanceamentoResponse>> RebalancearPorMudancaCesta([FromBody] RebalancearMudancaCestaRequest request)
  {
    try
    {
      var result = await _rebalanceamentoService.RebalancearPorMudancaCestaAsync(request);
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
    catch (NotFoundException ex)
    {
      return NotFound(new ErrorResponse
      {
        Erro = ex.Message,
        Codigo = ex.ErrorCode
      });
    }
    catch (Exception)
    {
      return BadRequest(new ErrorResponse
      {
        Erro = "Erro ao processar rebalanceamento.",
        Codigo = "ERRO_REBALANCEAMENTO"
      });
    }
  }
}
