using Microsoft.AspNetCore.Mvc;
using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Application.DTOs.Admin;
using CompraProgramadaAcoes.Application.Interfaces.Services;
using CompraProgramadaAcoes.Application.Exceptions;

namespace CompraProgramadaAcoes.Api.Controllers;

/// <summary>
/// Controller para operações administrativas do sistema
/// </summary>
[ApiController]
[Route("api/admin")]
public class AdminController(IAdminService adminService) : ControllerBase
{
  private readonly IAdminService _adminService = adminService;

  /// <summary>
  /// Cadastra ou altera a cesta Top Five
  /// </summary>
  /// <param name="request">Dados da cesta a ser cadastrada</param>
  /// <returns>Cesta cadastrada com sucesso</returns>
  /// <response code="201">Cesta cadastrada com sucesso</response>
  /// <response code="400">Dados inválidos ou erro de negócio</response>
  [HttpPost("cesta")]
  [ProducesResponseType(typeof(CestaAdminResponse), StatusCodes.Status201Created)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
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

  /// <summary>
  /// Consulta cesta atual
  /// </summary>
  /// <returns>Cesta Top Five atual</returns>
  /// <response code="200">Cesta atual retornada com sucesso</response>
  /// <response code="404">Nenhuma cesta encontrada</response>
  [HttpGet("cesta/atual")]
  [ProducesResponseType(typeof(CestaAtualResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
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

  /// <summary>
  /// Histórico de cestas
  /// </summary>
  /// <returns>Histórico completo de cestas Top Five</returns>
  /// <response code="200">Histórico retornado com sucesso</response>
  [HttpGet("cesta/historico")]
  [ProducesResponseType(typeof(CestasHistoricoResponse), StatusCodes.Status200OK)]
  public async Task<ActionResult<CestasHistoricoResponse>> ObterHistoricoCestas()
  {
    var result = await _adminService.ObterHistoricoCestasAsync();
    return Ok(result);
  }

  /// <summary>
  /// Consultar custódia master
  /// </summary>
  /// <returns>Posição consolidada da conta master</returns>
  /// <response code="200">Custódia retornada com sucesso</response>
  /// <response code="404">Custódia não encontrada</response>
  [HttpGet("conta-master/custodia")]
  [ProducesResponseType(typeof(ContaMasterCustodiaResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
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
