using Microsoft.AspNetCore.Mvc;
using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Application.DTOs.Motor;
using CompraProgramadaAcoes.Application.Interfaces.Services;
using CompraProgramadaAcoes.Application.Exceptions;

namespace CompraProgramadaAcoes.Api.Controllers;

[ApiController]
[Route("api/motor")]
public class MotorController(IMotorService motorService) : ControllerBase
{
  private readonly IMotorService _motorService = motorService;

  /// Executar compra manualmente (para testes)
  [HttpPost("executar-compra")]
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
