using Microsoft.AspNetCore.Mvc;
using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Application.DTOs.Admin;
using CompraProgramadaAcoes.Application.Interfaces.Services;
using CompraProgramadaAcoes.Application.Exceptions;

namespace CompraProgramadaAcoes.Api.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController(IAdminService adminService) : ControllerBase
{
  private readonly IAdminService _adminService = adminService;

  /// Cadastra ou altera a cesta Top Five
  [HttpPost("cesta")]
  public async Task<ActionResult<CestaAdminResponse>> CadastrarCesta([FromBody] CadastrarCestaAdminRequest request)
  {
    try
    {
      var result = await _adminService.CadastrarCestaAsync(request);
      return CreatedAtAction(nameof(ObterCestaAtual), new { }, result);
    }
    catch (BusinessException ex)
    {
      return BadRequest(new ErrorResponse
      {
        Erro = ex.Message,
        Codigo = ex.ErrorCode
      });
    }
  }

  /// Consulta cesta atual
  [HttpGet("cesta/atual")]
  public async Task<ActionResult<CestaAtualResponse>> ObterCestaAtual()
  {
    try
    {
      var result = await _adminService.ObterCestaAtualAsync();
      return Ok(result);
    }
    catch (NotFoundException ex)
    {
      return NotFound(new ErrorResponse
      {
        Erro = ex.Message,
        Codigo = ex.ErrorCode
      });
    }
  }

  /// Histórico de cestas
  [HttpGet("cesta/historico")]
  public async Task<ActionResult<CestasHistoricoResponse>> ObterHistoricoCestas()
  {
    var result = await _adminService.ObterHistoricoCestasAsync();
    return Ok(result);
  }

  /// Consultar custódia master
  [HttpGet("conta-master/custodia")]
  public async Task<ActionResult<ContaMasterCustodiaResponse>> ObterCustodiaMaster()
  {
    try
    {
      var result = await _adminService.ObterCustodiaMasterAsync();
      return Ok(result);
    }
    catch (NotFoundException ex)
    {
      return NotFound(new ErrorResponse
      {
        Erro = ex.Message,
        Codigo = ex.ErrorCode
      });
    }
  }
}
