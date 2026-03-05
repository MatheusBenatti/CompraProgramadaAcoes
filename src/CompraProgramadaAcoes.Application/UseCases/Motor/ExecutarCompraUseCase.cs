using CompraProgramadaAcoes.Application.DTOs.Motor;
using CompraProgramadaAcoes.Application.Interfaces;
using CompraProgramadaAcoes.Application.Exceptions;

namespace CompraProgramadaAcoes.Application.UseCases.Motor;

public class ExecutarCompraUseCase
{
    private readonly IMotorCompraProgramada _motorCompra;
    private readonly IOrdemCompraRepository _ordemCompraRepository;
    private readonly IDistribuicaoRepository _distribuicaoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IContaMasterRepository _contaMasterRepository;
    private readonly IEventoIRRepository _eventoIRRepository;

    public ExecutarCompraUseCase(
        IMotorCompraProgramada motorCompra,
        IOrdemCompraRepository ordemCompraRepository,
        IDistribuicaoRepository distribuicaoRepository,
        IClienteRepository clienteRepository,
        IContaMasterRepository contaMasterRepository,
        IEventoIRRepository eventoIRRepository)
    {
        _motorCompra = motorCompra;
        _ordemCompraRepository = ordemCompraRepository;
        _distribuicaoRepository = distribuicaoRepository;
        _clienteRepository = clienteRepository;
        _contaMasterRepository = contaMasterRepository;
        _eventoIRRepository = eventoIRRepository;
    }

    public async Task<ExecucaoCompraResponse> ExecuteAsync(ExecutarCompraRequest request)
    {
        // Validar data de referência
        if (!DateTime.TryParse(request.DataReferencia, out var dataReferencia))
        {
            throw new BusinessException("Data de referência inválida.", "DATA_REFERENCIA_INVALIDA");
        }

        // Executar compras programadas
        await _motorCompra.ExecutarComprasProgramadasAsync(dataReferencia);

        // Buscar dados reais do banco para montar a resposta
        var ordensCompra = await _ordemCompraRepository.ObterPorDataReferenciaAsync(dataReferencia);
        var distribuicoes = await _distribuicaoRepository.ObterPorDataReferenciaAsync(dataReferencia);
        var clientes = await _clienteRepository.ObterClientesAtivosAsync();
        var residuosMaster = await _contaMasterRepository.ObterResiduosAsync();
        var eventosIR = await _eventoIRRepository.ObterPorDataReferenciaAsync(dataReferencia);

        return new ExecucaoCompraResponse
        {
            DataExecucao = DateTime.UtcNow,
            TotalClientes = clientes.Count,
            TotalConsolidado = distribuicoes.Sum(d => d.ValorTotal),
            OrdensCompra = ordensCompra.Select(o => new OrdemCompraResponse
            {
                Ticker = o.Ticker,
                QuantidadeTotal = o.QuantidadeTotal,
                Detalhes = o.Detalhes.Select(d => new OrdemCompraDetalheResponse
                {
                    Tipo = d.Tipo,
                    Ticker = d.Ticker,
                    Quantidade = d.Quantidade
                }).ToList(),
                PrecoUnitario = o.PrecoUnitario,
                ValorTotal = o.ValorTotal
            }).ToList(),
            Distribuicoes = distribuicoes.Select(d => new DistribuicaoResponse
            {
                ClienteId = d.ClienteId,
                Nome = d.Cliente?.Nome ?? "Cliente não encontrado",
                ValorAporte = d.ValorAporte,
                Ativos = d.AtivosDistribuidos.Select(a => new AtivoDistribuidoResponse
                {
                    Ticker = a.Ticker,
                    Quantidade = a.Quantidade
                }).ToList()
            }).ToList(),
            ResiduosCustMaster = residuosMaster.Select(r => new ResiduoResponse
            {
                Ticker = r.Ticker,
                Quantidade = r.Quantidade
            }).ToList(),
            EventosIRPublicados = eventosIR.Count,
            Mensagem = $"Compra programada executada com sucesso para {clientes.Count} clientes."
        };
    }
}
