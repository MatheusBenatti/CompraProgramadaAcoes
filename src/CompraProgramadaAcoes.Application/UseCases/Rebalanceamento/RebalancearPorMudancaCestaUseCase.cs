using CompraProgramadaAcoes.Application.DTOs.Rebalanceamento;
using CompraProgramadaAcoes.Application.Interfaces;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Application.Exceptions;
using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Application.UseCases.Rebalanceamento;

public class RebalancearPorMudancaCestaUseCase
{
    private readonly IMotorRebalanceamento _motorRebalanceamento;
    private readonly ICestaRecomendacaoRepository _cestaRepository;
    private readonly IClienteRepository _clienteRepository;

    public RebalancearPorMudancaCestaUseCase(
        IMotorRebalanceamento motorRebalanceamento,
        ICestaRecomendacaoRepository cestaRepository,
        IClienteRepository clienteRepository)
    {
        _motorRebalanceamento = motorRebalanceamento;
        _cestaRepository = cestaRepository;
        _clienteRepository = clienteRepository;
    }

    public async Task<RebalanceamentoResponse> ExecuteAsync(RebalancearMudancaCestaRequest request)
    {
        // Validar IDs das cestas
        if (request.CestaAntigaId <= 0 || request.CestaNovaId <= 0)
        {
            throw new BusinessException("IDs das cestas devem ser válidos.", "IDS_CESTAS_INVALIDOS");
        }

        // Buscar cestas
        var cestaAntiga = await _cestaRepository.ObterPorIdAsync(request.CestaAntigaId);
        var cestaNova = await _cestaRepository.ObterPorIdAsync(request.CestaNovaId);

        if (cestaAntiga == null)
        {
            throw new NotFoundException("Cesta antiga não encontrada.", "CESTA_ANTIGA_NAO_ENCONTRADA");
        }

        if (cestaNova == null)
        {
            throw new NotFoundException("Cesta nova não encontrada.", "CESTA_NOVA_NAO_ENCONTRADA");
        }

        // Identificar diferenças entre as cestas
        var tickersAntigos = cestaAntiga.Itens.Select(i => i.Ticker).ToHashSet();
        var tickersNovos = cestaNova.Itens.Select(i => i.Ticker).ToHashSet();

        var ativosRemovidos = tickersAntigos.Except(tickersNovos).ToList();
        var ativosAdicionados = tickersNovos.Except(tickersAntigos).ToList();

        // Buscar clientes ativos que serão afetados
        var clientesAtivos = await _clienteRepository.ObterClientesAtivosAsync();

        // Executar rebalanceamento
        await _motorRebalanceamento.RebalancearPorMudancaCestaAsync(cestaAntiga, cestaNova);

        return new RebalanceamentoResponse
        {
            DataExecucao = DateTime.UtcNow,
            Mensagem = "Rebalanceamento por mudança de cesta iniciado com sucesso",
            Sucesso = true,
            TotalClientesAfetados = clientesAtivos.Count,
            AtivosRemovidos = ativosRemovidos,
            AtivosAdicionados = ativosAdicionados
        };
    }
}
