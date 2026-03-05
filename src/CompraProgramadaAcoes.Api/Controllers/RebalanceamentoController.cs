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
}

public class RebalancearMudancaCestaRequest
{
    public long CestaAntigaId { get; set; }
    public long CestaNovaId { get; set; }
}
