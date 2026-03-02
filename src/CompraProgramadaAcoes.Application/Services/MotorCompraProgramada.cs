using CompraProgramadaAcoes.Application.Interfaces;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CompraProgramadaAcoes.Application.Services;

public class MotorCompraProgramada(
      IClienteRepository clienteRepository,
      IContaMasterRepository contaMasterRepository,
      ICustodiaRepository custodiaRepository,
      ICestaRecomendacaoRepository cestaRepository,
      IOrdemCompraRepository ordemCompraRepository,
      IDistribuicaoRepository distribuicaoRepository,
      IMessagePublisher messagePublisher,
      CotahistParser cotahistParser,
      ILogger<MotorCompraProgramada> logger,
      string pastaCotacoes = "cotacoes") : IMotorCompraProgramada
{
    private readonly IClienteRepository _clienteRepository = clienteRepository;
    private readonly IContaMasterRepository _contaMasterRepository = contaMasterRepository;
    private readonly ICustodiaRepository _custodiaRepository = custodiaRepository;
    private readonly ICestaRecomendacaoRepository _cestaRepository = cestaRepository;
    private readonly IOrdemCompraRepository _ordemCompraRepository = ordemCompraRepository;
    private readonly IDistribuicaoRepository _distribuicaoRepository = distribuicaoRepository;
    private readonly IMessagePublisher _messagePublisher = messagePublisher;
    private readonly CotahistParser _cotahistParser = cotahistParser;
    private readonly ILogger<MotorCompraProgramada> _logger = logger;
  private readonly string _pastaCotacoes = pastaCotacoes;

  public async Task ExecutarComprasProgramadasAsync(DateTime dataReferencia)
    {
        try
        {
            // 1. Verificar se deve executar hoje
            if (!await DeveExecutarHoje(dataReferencia))
                return;

            // 2. Obter clientes ativos
            var clientesAtivos = await _clienteRepository.ObterClientesAtivosAsync();
            
            if (!clientesAtivos.Any())
            {
                _logger.LogInformation("Nenhum cliente ativo encontrado para execução de compras");
                return;
            }

            // 3. Obter cesta vigente
            var cestaVigente = await _cestaRepository.ObterCestaVigenteAsync();
            if (cestaVigente == null)
            {
                _logger.LogWarning("Nenhuma cesta vigente encontrada");
                return;
            }

            // 4. Calcular valor total do aporte do dia (1/3 do valor mensal)
            var valorTotalAporte = clientesAtivos.Sum(c => c.ValorMensal / 3);

            // 5. Obter cotações de fechamento dos ativos da cesta
            var tickers = cestaVigente.Itens.Select(i => i.Ticker);
            var cotacoes = _cotahistParser.ObterCotacoesFechamento(_pastaCotacoes, tickers);
            
            if (cotacoes.Count != tickers.Count())
            {
                _logger.LogError("Não foi possível obter todas as cotações necessárias");
                return;
            }

            // 6. Calcular compras consolidadas
            var comprasCalculadas = CalcularComprasConsolidadas(cestaVigente, cotacoes, valorTotalAporte);

            // 7. Obter conta master e verificar saldo
            var contaMaster = await _contaMasterRepository.ObterContaMasterAsync();
            
            // 8. Considerar saldo da custódia master
            var comprasAjustadas = AjustarComprasComSaldoMaster(comprasCalculadas, contaMaster);

            // 9. Executar ordens de compra na conta master
            var ordensExecutadas = await ExecutarComprasMaster(contaMaster, comprasAjustadas);

            // 10. Distribuir ativos para os clientes
            await DistribuirAtivosParaClientes(ordensExecutadas, clientesAtivos, cestaVigente);

            // 11. Salvar alterações
            await _custodiaRepository.SaveChangesAsync();

            _logger.LogInformation($"Execução de compras programadas concluída. {ordensExecutadas.Count} ordens executadas.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante execução de compras programadas");
            throw;
        }
    }

    public Task<bool> DeveExecutarHoje(DateTime data)
    {
        // Verificar se é dia útil (segunda a sexta)
        if (data.DayOfWeek == DayOfWeek.Saturday || data.DayOfWeek == DayOfWeek.Sunday)
            return Task.FromResult(false);

        // Verificar se é dia 5, 15 ou 25
        var dia = data.Day;
        return Task.FromResult(dia == 5 || dia == 15 || dia == 25);
    }

    private Dictionary<string, (int Quantidade, decimal Valor)> CalcularComprasConsolidadas(
        CestaRecomendacao cesta, Dictionary<string, CotacaoB3> cotacoes, decimal valorTotal)
    {
        var resultado = new Dictionary<string, (int Quantidade, decimal Valor)>();

        foreach (var item in cesta.Itens)
        {
            var cotacao = cotacoes[item.Ticker];
            var valorPorAtivo = valorTotal * (item.Percentual / 100);
            var quantidade = (int)Math.Floor(valorPorAtivo / cotacao.PrecoFechamento);
            var valorReal = quantidade * cotacao.PrecoFechamento;

            resultado[item.Ticker] = (quantidade, valorReal);
        }

        return resultado;
    }

    private Dictionary<string, (int Quantidade, decimal Valor)> AjustarComprasComSaldoMaster(
        Dictionary<string, (int Quantidade, decimal Valor)> compras, ContaGrafica contaMaster)
    {
        var resultado = new Dictionary<string, (int Quantidade, decimal Valor)>();

        foreach (var compra in compras)
        {
            var ticker = compra.Key;
            var quantidadeDesejada = compra.Value.Quantidade;
            
            // Obter saldo disponível da custódia master
            var custodiaMaster = _custodiaRepository.ObterPorTickerMasterAsync(contaMaster.Id, ticker).Result;
            var saldoDisponivel = custodiaMaster?.Quantidade ?? 0;

            var quantidadeFinal = Math.Max(0, quantidadeDesejada - saldoDisponivel);
            var preco = _cotahistParser.ObterCotacaoFechamento(_pastaCotacoes, ticker)?.PrecoFechamento ?? 0;
            var valorFinal = quantidadeFinal * preco;

            resultado[ticker] = (quantidadeFinal, valorFinal);
        }

        return resultado;
    }

    private async Task<List<OrdemCompra>> ExecutarComprasMaster(
        ContaGrafica contaMaster, Dictionary<string, (int Quantidade, decimal Valor)> compras)
    {
        var ordens = new List<OrdemCompra>();

        foreach (var compra in compras)
        {
            if (compra.Value.Quantidade <= 0) continue;

            var ticker = compra.Key;
            var quantidadeTotal = compra.Value.Quantidade;
            var preco = _cotahistParser.ObterCotacaoFechamento(_pastaCotacoes, ticker)?.PrecoFechamento ?? 0;

            // Criar ordem de compra (priorizar lotes padrão)
            var quantidadeLotesPadrao = (quantidadeTotal / 100) * 100;
            var quantidadeFracionaria = quantidadeTotal % 100;

            if (quantidadeLotesPadrao > 0)
            {
                var ordemLotePadrao = new OrdemCompra(contaMaster.Id, ticker, quantidadeLotesPadrao, preco, TipoMercado.Lote);
                ordemLotePadrao.AtualizarDataExecucao(DateTime.UtcNow);
                ordens.Add(ordemLotePadrao);
            }

            if (quantidadeFracionaria > 0)
            {
                var ordemFracionaria = new OrdemCompra(contaMaster.Id, ticker, quantidadeFracionaria, preco, TipoMercado.Fracionario);
                ordemFracionaria.AtualizarDataExecucao(DateTime.UtcNow);
                ordens.Add(ordemFracionaria);
            }
        }

        // Salvar ordens
        foreach (var ordem in ordens)
        {
            await _ordemCompraRepository.AddAsync(ordem);
        }

        await _ordemCompraRepository.SaveChangesAsync();

        return ordens;
    }

    private async Task DistribuirAtivosParaClientes(
        List<OrdemCompra> ordens, List<Cliente> clientes, CestaRecomendacao cesta)
    {
        foreach (var cliente in clientes)
        {
            var valorAporteCliente = cliente.ValorMensal / 3;
            var contaGrafica = cliente.ContaGrafica;

            foreach (var ordem in ordens)
            {
                // Calcular quantidade proporcional para o cliente
                var percentualCliente = (decimal)(valorAporteCliente / clientes.Sum(c => c.ValorMensal * 3));
                var quantidadeCliente = (int)Math.Floor(ordem.Quantidade * percentualCliente);

                if (quantidadeCliente <= 0) continue;

                // Criar distribuição
                var distribuicao = new Distribuicao(ordem.Id, contaGrafica.Id, ordem.Ticker, quantidadeCliente, ordem.PrecoUnitario);
                distribuicao.AtualizarDataDistribuicao(DateTime.UtcNow);

                await _distribuicaoRepository.AddAsync(distribuicao);

                // Atualizar custódia do cliente
                var custodiaCliente = await _custodiaRepository.ObterPorTickerAsync(contaGrafica.Id, ordem.Ticker);
                if (custodiaCliente == null)
                {
                    custodiaCliente = new Custodia(contaGrafica.Id);
                    custodiaCliente.AtualizarCustodia(quantidadeCliente, ordem.PrecoUnitario, ordem.Ticker);
                    await _custodiaRepository.AddAsync(custodiaCliente);
                }
                else
                {
                    // Atualizar preço médio e quantidade
                    var quantidadeAnterior = custodiaCliente.Quantidade;
                    var valorAnterior = quantidadeAnterior * custodiaCliente.PrecoMedio;
                    var valorNovo = quantidadeCliente * ordem.PrecoUnitario;
                    var quantidadeTotal = quantidadeAnterior + quantidadeCliente;
                    var precoMedioNovo = (valorAnterior + valorNovo) / quantidadeTotal;

                    custodiaCliente.AtualizarCustodia(quantidadeTotal, precoMedioNovo);
                }

                // Publicar evento IR dedo-duro
                var valorOperacao = quantidadeCliente * ordem.PrecoUnitario;
                var valorIR = valorOperacao * 0.0005m; // 0,005%

                var irEvent = new
                {
                    Tipo = "IrDeduDuro",
                    ClienteId = cliente.Id,
                    Ticker = ordem.Ticker,
                    Quantidade = quantidadeCliente,
                    ValorOperacao = valorOperacao,
                    IrRetido = valorIR,
                    DataOperacao = DateTime.UtcNow
                };

                await _messagePublisher.PublishAsync("ir-events", System.Text.Json.JsonSerializer.Serialize(irEvent));
            }
        }
    }
}
