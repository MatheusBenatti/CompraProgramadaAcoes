using CompraProgramadaAcoes.Application.DTOs.Motor;
using CompraProgramadaAcoes.Application.Interfaces;
using CompraProgramadaAcoes.Application.Exceptions;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;

namespace CompraProgramadaAcoes.Application.UseCases.Motor;

public class ExecutarCompraUseCase(
    IMotorCompraProgramada motorCompra,
    IOrdemCompraRepository ordemCompraRepository,
    IDistribuicaoRepository distribuicaoRepository,
    IClienteRepository clienteRepository,
    IContaMasterRepository contaMasterRepository,
    IEventoIRRepository eventoIRRepository)
{
  private readonly IMotorCompraProgramada _motorCompra = motorCompra;
  private readonly IOrdemCompraRepository _ordemCompraRepository = ordemCompraRepository;
  private readonly IDistribuicaoRepository _distribuicaoRepository = distribuicaoRepository;
  private readonly IClienteRepository _clienteRepository = clienteRepository;
  private readonly IContaMasterRepository _contaMasterRepository = contaMasterRepository;
  private readonly IEventoIRRepository _eventoIRRepository = eventoIRRepository;

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
      TotalConsolidado = distribuicoes.Sum(d => d.Quantidade * d.PrecoUnitario),
      OrdensCompra = [.. ordensCompra.Select(o => new OrdemCompraResponse
      {
        Ticker = o.Ticker,
        QuantidadeTotal = o.Quantidade,
        Detalhes =
        [
          new() {
            Tipo = o.TipoMercado.ToString(),
            Ticker = o.Ticker,
            Quantidade = o.Quantidade
          }
        ],
        PrecoUnitario = o.PrecoUnitario,
        ValorTotal = o.Quantidade * o.PrecoUnitario
      })],
      Distribuicoes = [.. distribuicoes
        .GroupBy(d => d.CustodiaFilhote.ContaGrafica.ClienteId)
        .Select(g => new DistribuicaoResponse
        {
          ClienteId = g.Key ?? 0,
          Nome = g.FirstOrDefault()?.CustodiaFilhote?.ContaGrafica?.Cliente?.Nome ?? "Cliente não encontrado",
          ValorAporte = g.Sum(d => d.Quantidade * d.PrecoUnitario),
          Ativos = [.. g.GroupBy(d => d.Ticker)
            .Select(a => new AtivoDistribuidoResponse
            {
              Ticker = a.Key,
              Quantidade = a.Sum(d => d.Quantidade)
            })]
        })],
      ResiduosCustMaster = [.. residuosMaster.Select(r => new ResiduoResponse { Ticker = r.Ticker!, Quantidade = r.Quantidade })],
      EventosIRPublicados = eventosIR.Count,
      Mensagem = $"Compra programada executada com sucesso para {clientes.Count} clientes."
    };
  }
}
