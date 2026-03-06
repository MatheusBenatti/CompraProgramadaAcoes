using CompraProgramadaAcoes.Application.Interfaces;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace CompraProgramadaAcoes.Application.Services;

public class MotorCompraProgramada(
      IClienteRepository clienteRepository,
      IContaMasterRepository contaMasterRepository,
      ICustodiaRepository custodiaRepository,
      ICestaCacheService cestaCacheService,
      CotacaoCacheService cotacaoCacheService,
      IOrdemCompraRepository ordemCompraRepository,
      IDistribuicaoRepository distribuicaoRepository,
      IMessagePublisher messagePublisher,
      CotahistParser cotahistParser,
      ILogger<MotorCompraProgramada> logger,
      IConfiguration configuration,
      IWebHostEnvironment env) : IMotorCompraProgramada
{
  private readonly IClienteRepository _clienteRepository = clienteRepository;
  private readonly IContaMasterRepository _contaMasterRepository = contaMasterRepository;
  private readonly ICustodiaRepository _custodiaRepository = custodiaRepository;
  private readonly ICestaCacheService _cestaCacheService = cestaCacheService;
  private readonly CotacaoCacheService _cotacaoCacheService = cotacaoCacheService;
  private readonly IOrdemCompraRepository _ordemCompraRepository = ordemCompraRepository;
  private readonly IDistribuicaoRepository _distribuicaoRepository = distribuicaoRepository;
  private readonly IMessagePublisher _messagePublisher = messagePublisher;
  private readonly CotahistParser _cotahistParser = cotahistParser;
  private readonly ILogger<MotorCompraProgramada> _logger = logger;
  private readonly IConfiguration _configuration = configuration;
  private readonly IWebHostEnvironment _env = env;

  /// Obtém o caminho completo para a pasta de cotações
  private string ObterCaminhoCotacoes()
  {
    var path = _configuration["FileStorage:CotacoesPath"] ?? "cotacoes";
    return Path.GetFullPath(Path.Combine(_env.ContentRootPath, path));
  }

  public async Task ExecutarComprasProgramadasAsync(DateTime dataReferencia)
  {
    try
    {
      // 1. Verificar se deve executar hoje
      if (!await DeveExecutarHoje(dataReferencia))
        return;

      // 2. Obter clientes ativos
      var clientesAtivos = await _clienteRepository.ObterClientesAtivosAsync();

      if (clientesAtivos.Count == 0)
      {
        _logger.LogInformation("Nenhum cliente ativo encontrado para execução de compras");
        return;
      }

      // 3. Obter cesta vigente do Redis
      var cestaVigente = await _cestaCacheService.ObterCestaAsync();
      if (cestaVigente == null)
      {
        _logger.LogWarning("Nenhuma cesta vigente encontrada no Redis");
        return;
      }

      // 4. Calcular valor total do aporte do dia (1/3 do valor mensal)
      var valorTotalAporte = clientesAtivos.Sum(c => c.ValorMensal / 3);

      // 5. Obter cotações de fechamento dos ativos da cesta do Redis
      var tickers = cestaVigente.Itens.Select(i => i.Ticker);
      var precosFechamento = await _cotacaoCacheService.ObterPrecosFechamentoAsync(tickers);

      if (precosFechamento.Count != tickers.Count())
      {
        _logger.LogError("Não foi possível obter todas as cotações necessárias do Redis");
        return;
      }

      // 6. Calcular compras consolidadas
      var comprasCalculadas = CalcularComprasConsolidadas(cestaVigente, precosFechamento, valorTotalAporte);

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
    var ehDiaDeExecucao = dia == 5 || dia == 15 || dia == 25;

    if (!ehDiaDeExecucao)
      return Task.FromResult(false);

    // TODO: Implementar validação de feriados brasileiros
    // Por enquanto, considera apenas fins de semana

    return Task.FromResult(true);
  }

  private Dictionary<string, (int Quantidade, decimal Valor)> CalcularComprasConsolidadas(
      CestaCacheDTO cesta, Dictionary<string, decimal> precos, decimal valorTotal)
  {
    var resultado = new Dictionary<string, (int Quantidade, decimal Valor)>();

    foreach (var item in cesta.Itens)
    {
      if (!precos.TryGetValue(item.Ticker, out var preco))
      {
        _logger.LogError($"Cotação não encontrada para o ticker {item.Ticker}");
        continue;
      }

      var valorPorAtivo = valorTotal * (item.Percentual / 100);
      var quantidade = preco > 0 ? (int)Math.Floor(valorPorAtivo / preco) : 0;
      var valorReal = quantidade * preco;

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
      var preco = _cotahistParser.ObterCotacaoFechamento(ObterCaminhoCotacoes(), ticker)?.PrecoFechamento ?? 0;
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
      var preco = _cotahistParser.ObterCotacaoFechamento(ObterCaminhoCotacoes(), ticker)?.PrecoFechamento ?? 0;

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
      List<OrdemCompra> ordens, List<Cliente> clientes, CestaCacheDTO cesta)
  {
    // Calcular total de aportes do dia (1/3 de cada cliente)
    var totalAportes = clientes.Sum(c => c.ValorMensal / 3);

    _logger.LogInformation($"Iniciando distribuição de {ordens.Count} ordens para {clientes.Count} clientes. Total de aportes: R$ {totalAportes:N2}");

    foreach (var cliente in clientes)
    {
      var valorAporteCliente = cliente.ValorMensal / 3;
      var contaGrafica = cliente.ContaGrafica;

      // Calcular percentual correto do cliente sobre o total
      var percentualCliente = valorAporteCliente / totalAportes;

      foreach (var ordem in ordens)
      {
        // Obter percentual do ativo na cesta
        var itemCesta = cesta.Itens.FirstOrDefault(i => i.Ticker == ordem.Ticker);
        if (itemCesta == null) continue;

        // Calcular quantidade baseada no valor do aporte do cliente para este ativo
        var valorAporteAtivo = valorAporteCliente * (itemCesta.Percentual / 100);
        var quantidadeCliente = (int)Math.Floor(valorAporteAtivo / ordem.PrecoUnitario);

        if (quantidadeCliente <= 0) continue;

        // Log da distribuição
        _logger.LogInformation($"Distribuindo {quantidadeCliente} ações de {ordem.Ticker} para cliente {cliente.Nome} (ID: {cliente.Id})");

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
          ordem.Ticker,
          Quantidade = quantidadeCliente,
          ValorOperacao = valorOperacao,
          IrRetido = valorIR,
          DataOperacao = DateTime.UtcNow
        };

        await _messagePublisher.PublishAsync("ir-events", System.Text.Json.JsonSerializer.Serialize(irEvent));
      }
    }

    // Calcular e logar resíduos da distribuição
    foreach (var ordem in ordens)
    {
      var totalDistribuido = 0;
      foreach (var cliente in clientes)
      {
        var itemCesta = cesta.Itens.FirstOrDefault(i => i.Ticker == ordem.Ticker);
        if (itemCesta == null) continue;

        var valorAporteCliente = cliente.ValorMensal / 3;
        var valorAporteAtivo = valorAporteCliente * (itemCesta.Percentual / 100);
        var quantidadeCliente = (int)Math.Floor(valorAporteAtivo / ordem.PrecoUnitario);
        totalDistribuido += quantidadeCliente;
      }

      var residuo = ordem.Quantidade - totalDistribuido;
      if (residuo > 0)
      {
        _logger.LogInformation($"Resíduo de {residuo} ações de {ordem.Ticker} mantido na conta master");
      }
    }
  }
}
