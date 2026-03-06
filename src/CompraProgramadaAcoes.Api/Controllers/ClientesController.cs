using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Application.Exceptions;
using CompraProgramadaAcoes.Application.Interfaces.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace CompraProgramadaAcoes.Api.Controllers;

/// <summary>
/// Controller para operações de clientes
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ClientesController(IRealizarAdesao realizarAdesao, IRealizarSaida realizarSaida, IAlterarValorMensal alterarValorMensal, IConsultarCarteira consultarCarteira, IConsultarRentabilidade consultarRentabilidade) : ControllerBase
{
  private readonly IRealizarAdesao _realizarAdesao = realizarAdesao;
  private readonly IRealizarSaida _realizarSaida = realizarSaida;
  private readonly IAlterarValorMensal _alterarValorMensal = alterarValorMensal;
  private readonly IConsultarCarteira _consultarCarteira = consultarCarteira;
  private readonly IConsultarRentabilidade _consultarRentabilidade = consultarRentabilidade;

  /// Realiza a adesão de um novo cliente ao produto
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
  /// <returns>Resultado da saída do cliente</returns>
  /// <response code="200">Saída realizada com sucesso</response>
  /// <response code="400">Cliente já inativo</response>
  /// <response code="404">Cliente não encontrado</response>
  /// <response code="500">Erro interno</response>
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
    catch (ClienteJaInativoException)
    {
      return BadRequest(new ErrorResponse
      {
        Erro = "Cliente já havia saído do produto.",
        Codigo = "CLIENTE_JA_INATIVO"
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

  /// <summary>
  /// Altera o valor mensal de um cliente
  /// </summary>
  /// <param name="clienteId">ID do cliente</param>
  /// <param name="request">Dados da alteração do valor mensal</param>
  /// <returns>Valor mensal alterado com sucesso</returns>
  /// <response code="200">Valor alterado com sucesso</response>
  /// <response code="400">Dados inválidos ou cliente já inativo</response>
  /// <response code="404">Cliente não encontrado</response>
  /// <response code="500">Erro interno</response>
  [HttpPut("{clienteId}/valor-mensal")]
  [ProducesResponseType(typeof(AlterarValorMensalResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> AlterarValorMensal(int clienteId, [FromBody] AlterarValorMensalRequest request)
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
      var response = await _alterarValorMensal.ExecuteAsync(clienteId, request);
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
    catch (ClienteJaInativoException)
    {
      return BadRequest(new ErrorResponse
      {
        Erro = "Cliente já havia saído do produto.",
        Codigo = "CLIENTE_JA_INATIVO"
      });
    }
    catch (Exception)
    {
      return StatusCode(500, new ErrorResponse
      {
        Erro = "Erro interno ao processar alteração de valor mensal.",
        Codigo = "ERRO_INTERNO"
      });
    }
  }

  /// <summary>
  /// Consulta a carteira de ativos do cliente
  /// </summary>
  /// <param name="clienteId">ID do cliente</param>
  /// <returns>Carteira completa do cliente</returns>
  /// <response code="200">Carteira retornada com sucesso</response>
  /// <response code="404">Cliente não encontrado</response>
  /// <response code="500">Erro interno</response>
  [HttpGet("{clienteId}/carteira")]
  [ProducesResponseType(typeof(CarteiraResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> ConsultarCarteira(int clienteId)
  {
    try
    {
      var response = await _consultarCarteira.ExecuteAsync(clienteId);
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
        Erro = "Erro interno ao consultar carteira.",
        Codigo = "ERRO_INTERNO"
      });
    }
  }

  /// <summary>
  /// Consulta o acompanhamento detalhado de rentabilidade do cliente
  /// </summary>
  /// <param name="clienteId">ID do cliente</param>
  /// <returns>Dados detalhados de rentabilidade</returns>
  /// <response code="200">Rentabilidade retornada com sucesso</response>
  /// <response code="404">Cliente não encontrado</response>
  /// <response code="500">Erro interno</response>
  [HttpGet("{clienteId}/rentabilidade")]
  [ProducesResponseType(typeof(RentabilidadeResponse), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status500InternalServerError)]
  public async Task<IActionResult> ConsultarRentabilidade(int clienteId)
  {
    try
    {
      var response = await _consultarRentabilidade.ExecuteAsync(clienteId);
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
        Erro = "Erro interno ao consultar rentabilidade.",
        Codigo = "ERRO_INTERNO"
      });
    }
  }
}
