using Microsoft.AspNetCore.Mvc;
using CompraProgramadaAcoes.Application.Interfaces;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RebalanceamentoController : ControllerBase
{
    private readonly IMotorRebalanceamento _motorRebalanceamento;
    private readonly ICestaRecomendacaoRepository _cestaRepository;

    public RebalanceamentoController(
        IMotorRebalanceamento motorRebalanceamento,
        ICestaRecomendacaoRepository cestaRepository)
    {
        _motorRebalanceamento = motorRebalanceamento;
        _cestaRepository = cestaRepository;
    }

    /// <summary>
    /// Rebalanceia carteiras por mudança de cesta
    /// </summary>
    [HttpPost("mudanca-cesta")]
    public async Task<ActionResult> RebalancearPorMudancaCesta([FromBody] RebalancearMudancaCestaRequest request)
    {
        try
        {
            var cestaAntiga = await _cestaRepository.ObterPorIdAsync(request.CestaAntigaId);
            var cestaNova = await _cestaRepository.ObterPorIdAsync(request.CestaNovaId);

            if (cestaAntiga == null || cestaNova == null)
                return NotFound("Cesta(s) não encontrada(s)");

            await _motorRebalanceamento.RebalancearPorMudancaCestaAsync(cestaAntiga, cestaNova);

            return Ok("Rebalanceamento por mudança de cesta iniciado com sucesso");
        }
        catch (Exception ex)
        {
            return BadRequest($"Erro ao rebalancear: {ex.Message}");
        }
    }

    /// <summary>
    /// Rebalanceia carteiras por desvio de proporção
    /// </summary>
    [HttpPost("desvio-proporcao")]
    public async Task<ActionResult> RebalancearPorDesvioProporcao([FromBody] RebalancearDesvioRequest request)
    {
        try
        {
            var limiteDesvio = request.LimiteDesvioPercentual > 0 ? request.LimiteDesvioPercentual : 0.10m;
            
            await _motorRebalanceamento.RebalancearPorDesvioProporcaoAsync(limiteDesvio);

            return Ok($"Rebalanceamento por desvio de proporção iniciado com limite de {limiteDesvio:P1}");
        }
        catch (Exception ex)
        {
            return BadRequest($"Erro ao rebalancear: {ex.Message}");
        }
    }
}

public class RebalancearMudancaCestaRequest
{
    public long CestaAntigaId { get; set; }
    public long CestaNovaId { get; set; }
}

public class RebalancearDesvioRequest
{
    public decimal LimiteDesvioPercentual { get; set; } = 0.10m; // 10% padrão
}
