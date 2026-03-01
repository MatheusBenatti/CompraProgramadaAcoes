using Microsoft.AspNetCore.Mvc;
using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Application.UseCases;

namespace CompraProgramadaAcoes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController(RealizarAdesao realizarAdesao) : ControllerBase
{
  private readonly RealizarAdesao _realizarAdesao = realizarAdesao;

  /// <summary>
  /// Realiza a adesão de um novo cliente ao produto
  /// </summary>
  /// <param name="request">Dados de adesão do cliente</param>
  /// <returns>Dados do cliente criado com sua conta gráfica</returns>
  [HttpPost("adesao")]
  [ProducesResponseType(typeof(AdesaoResponse), StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status409Conflict)]
  public async Task<IActionResult> Adesao([FromBody] AdesaoRequest request)
  {
    if (!ModelState.IsValid)
      return BadRequest(ModelState);

    try
    {
      var response = await _realizarAdesao.ExecuteAsync(request);
      return CreatedAtAction(
          nameof(Adesao),
          new { id = response.ClienteId },
          response);
    }
    catch (InvalidOperationException ex) when (ex.Message.Contains("CPF já cadastrado"))
    {
      return Conflict(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
      return BadRequest(new { error = ex.Message });
    }
    catch (Exception ex)
    {
      return StatusCode(500, new { error = "Erro interno ao processar adesão" });
    }
  }
}
