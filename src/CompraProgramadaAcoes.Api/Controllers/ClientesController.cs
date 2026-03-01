using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Application.Exceptions;
using CompraProgramadaAcoes.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

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
    {
      return BadRequest(new ErrorResponse
      {
        Erro = "Dados inválidos.",
        Codigo = "REQUISICAO_INVALIDA"
      });
    }

    try
    {
      var response = await _realizarAdesao.ExecuteAsync(request);

      return CreatedAtAction(
          nameof(Adesao),
          new { id = response.ClienteId },
          response);
    }
    catch (ClienteCpfDuplicadoException ex)
    {
      return BadRequest(new ErrorResponse
      {
        Erro = ex.Message,
        Codigo = "CLIENTE_CPF_DUPLICADO"
      });
    }
    catch (ArgumentException ex)
    {
      return BadRequest(new ErrorResponse
      {
        Erro = ex.Message,
        Codigo = "VALIDACAO_ERRO"
      });
    }
    catch (Exception)
    {
      return StatusCode(500, new ErrorResponse
      {
        Erro = "Erro interno ao processar adesão.",
        Codigo = "ERRO_INTERNO"
      });
    }
  }
}
