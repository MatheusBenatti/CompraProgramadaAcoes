using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Application.Exceptions;
using CompraProgramadaAcoes.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace CompraProgramadaAcoes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController(RealizarAdesao realizarAdesao, RealizarSaida realizarSaida) : ControllerBase
{
  private readonly RealizarAdesao _realizarAdesao = realizarAdesao;
  private readonly RealizarSaida _realizarSaida = realizarSaida;

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

  /// <summary>
  /// Realiza a saída de um cliente do produto
  /// </summary>
  /// <param name="clienteId">ID do cliente</param>
  /// <returns>Dados da saída do cliente</returns>
  [HttpPost("{clienteId}/saida")]
  [ProducesResponseType(typeof(SaidaResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> Saida(int clienteId)
  {
    try
    {
      var response = await _realizarSaida.ExecuteAsync(clienteId);
      return Ok(response);
    }
    catch (ClienteNaoEncontradoException)
    {
      return NotFound(new ErrorResponse
      {
        Erro = "Cliente não encontrado.",
        Codigo = "CLIENTE_NAO_ENCONTRADO"
      });
    }
    catch (Exception)
    {
      return StatusCode(500, new ErrorResponse
      {
        Erro = "Erro interno ao processar saída.",
        Codigo = "ERRO_INTERNO"
      });
    }
  }
}
