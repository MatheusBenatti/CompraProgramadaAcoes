using Microsoft.AspNetCore.Mvc;
using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Application.DTOs.Rebalanceamento;
using CompraProgramadaAcoes.Application.Interfaces.Services;
using CompraProgramadaAcoes.Application.Exceptions;

namespace CompraProgramadaAcoes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RebalanceamentoController(IRebalanceamentoService rebalanceamentoService) : ControllerBase
{
  private readonly IRebalanceamentoService _rebalanceamentoService = rebalanceamentoService;

  /// Rebalanceia carteiras por mudança de cesta
  [HttpPost("mudanca-cesta")]
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
