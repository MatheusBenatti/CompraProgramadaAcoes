using CompraProgramadaAcoes.Application.Interfaces;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CompraProgramadaAcoes.Application.Services;

public class MotorRebalanceamento(
    IClienteRepository clienteRepository,
    ICustodiaRepository custodiaRepository,
    ICestaRecomendacaoRepository cestaRepository,
    IMessagePublisher messagePublisher,
    CotahistParser cotahistParser,
    ILogger<MotorRebalanceamento> logger,
    string pastaCotacoes = "cotacoes") : IMotorRebalanceamento
{
  private readonly IClienteRepository _clienteRepository = clienteRepository;
  private readonly ICustodiaRepository _custodiaRepository = custodiaRepository;
  private readonly ICestaRecomendacaoRepository _cestaRepository = cestaRepository;
  private readonly IMessagePublisher _messagePublisher = messagePublisher;
  private readonly CotahistParser _cotahistParser = cotahistParser;
  private readonly ILogger<MotorRebalanceamento> _logger = logger;
  private readonly string _pastaCotacoes = pastaCotacoes;

  public async Task RebalancearPorMudancaCestaAsync(CestaRecomendacao cestaAntiga, CestaRecomendacao cestaNova)
  {
    var clientesAtivos = await _clienteRepository.ObterClientesAtivosAsync();

    foreach (var cliente in clientesAtivos)
    {
      await RebalancearClientePorMudancaCesta(cliente, cestaAntiga, cestaNova);
    }

    await _custodiaRepository.SaveChangesAsync();
  }

  public async Task RebalancearPorDesvioProporcaoAsync(decimal limiteDesvioPercentual = 0.10m) // 10% por padrão
  {
    var cestaVigente = await _cestaRepository.ObterCestaVigenteAsync();
    if (cestaVigente == null)
    {
      _logger.LogWarning("Nenhuma cesta vigente encontrada para rebalanceamento");
      return;
    }

    var clientesAtivos = await _clienteRepository.ObterClientesAtivosAsync();

    foreach (var cliente in clientesAtivos)
    {
      await RebalancearClientePorDesvioProporcao(cliente, cestaVigente, limiteDesvioPercentual);
    }

    await _custodiaRepository.SaveChangesAsync();
  }

  private async Task RebalancearClientePorMudancaCesta(Cliente cliente, CestaRecomendacao cestaAntiga, CestaRecomendacao cestaNova)
  {
    var custodia = await _custodiaRepository.ObterPorContaGraficaAsync(cliente.ContaGrafica.Id);

    // Identificar ativos que saíram da cesta
    var ativosSairam = cestaAntiga.Itens
        .Where(itemAntigo => !cestaNova.Itens.Any(itemNovo => itemNovo.Ticker == itemAntigo.Ticker))
        .ToList();

    decimal valorTotalVendas = 0;

    // Vender ativos que saíram da cesta
    foreach (var itemSaiu in ativosSairam)
    {
      var posicao = custodia.FirstOrDefault(c => c.Ticker == itemSaiu.Ticker);
      if (posicao != null && posicao.Quantidade > 0)
      {
        var cotacao = _cotahistParser.ObterCotacaoFechamento(_pastaCotacoes, itemSaiu.Ticker);
        if (cotacao != null)
        {
          var valorVenda = posicao.Quantidade * cotacao.PrecoFechamento;
          valorTotalVendas += valorVenda;

          // Zerar posição do ativo
          posicao.AtualizarCustodia(0, 0);

          // Registrar venda para cálculo de IR
          await RegistrarVendaParaIR(cliente, itemSaiu.Ticker, posicao.Quantidade, cotacao.PrecoFechamento, posicao.PrecoMedio);
        }
      }
    }

    // Comprar novos ativos com o valor obtido
    if (valorTotalVendas > 0)
    {
      await ComprarAtivosNovaCesta(cliente, cestaNova, valorTotalVendas);
    }
  }

  private async Task RebalancearClientePorDesvioProporcao(Cliente cliente, CestaRecomendacao cesta, decimal limiteDesvio)
  {
    var custodia = await _custodiaRepository.ObterPorContaGraficaAsync(cliente.ContaGrafica.Id);
    if (custodia.Count == 0) return;

    // Obter cotações atuais
    var tickers = custodia.Select(c => c.Ticker).Union(cesta.Itens.Select(i => i.Ticker));
    var cotacoes = _cotahistParser.ObterCotacoesFechamento(_pastaCotacoes, tickers!);

    // Calcular valor total da carteira
    decimal valorTotalCarteira = 0;
    var proporcoesAtuais = new Dictionary<string, decimal>();

    foreach (var posicao in custodia.Where(c => c.Quantidade > 0))
    {
      if (cotacoes.TryGetValue(posicao.Ticker!, out var cotacao))
      {
        var valorPosicao = posicao.Quantidade * cotacao.PrecoFechamento;
        valorTotalCarteira += valorPosicao;
        proporcoesAtuais[posicao.Ticker!] = valorPosicao;
      }
    }

    // Identificar desvios
    var operacoesRebalanceamento = new List<object>();

    foreach (var itemCesta in cesta.Itens)
    {
      var proporcaoAlvo = itemCesta.Percentual / 100;
      var valorAlvo = valorTotalCarteira * proporcaoAlvo;

      if (proporcoesAtuais.TryGetValue(itemCesta.Ticker, out var valorAtual))
      {
        var proporcaoAtual = valorAtual / valorTotalCarteira;
        var desvio = Math.Abs(proporcaoAtual - proporcaoAlvo);

        if (desvio > limiteDesvio)
        {
          if (proporcaoAtual > proporcaoAlvo)
          {
            // Vender excesso
            var cotacao = cotacoes[itemCesta.Ticker];
            var valorVender = valorAtual - valorAlvo;
            var quantidadeVender = (int)Math.Floor(valorVender / cotacao.PrecoFechamento);

            if (quantidadeVender > 0)
            {
              operacoesRebalanceamento.Add(new
              {
                Tipo = "Venda",
                itemCesta.Ticker,
                Quantidade = quantidadeVender,
                Preco = cotacao.PrecoFechamento
              });
            }
          }
          else
          {
            // Comprar deficiência
            var cotacao = cotacoes[itemCesta.Ticker];
            var valorComprar = valorAlvo - valorAtual;
            var quantidadeComprar = (int)Math.Floor(valorComprar / cotacao.PrecoFechamento);

            if (quantidadeComprar > 0)
            {
              operacoesRebalanceamento.Add(new
              {
                Tipo = "Compra",
                itemCesta.Ticker,
                Quantidade = quantidadeComprar,
                Preco = cotacao.PrecoFechamento
              });
            }
          }
        }
      }
    }

    // Executar operações de rebalanceamento
    await ExecutarOperacoesRebalanceamento(cliente, operacoesRebalanceamento);
  }

  private async Task ComprarAtivosNovaCesta(Cliente cliente, CestaRecomendacao cesta, decimal valorDisponivel)
  {
    var cotacoes = _cotahistParser.ObterCotacoesFechamento(_pastaCotacoes, cesta.Itens.Select(i => i.Ticker));
    var custodia = await _custodiaRepository.ObterPorContaGraficaAsync(cliente.ContaGrafica.Id);

    foreach (var item in cesta.Itens)
    {
      if (!cotacoes.TryGetValue(item.Ticker, out var cotacao)) continue;

      var valorPorAtivo = valorDisponivel * (item.Percentual / 100);
      var quantidade = (int)Math.Floor(valorPorAtivo / cotacao.PrecoFechamento);

      if (quantidade <= 0) continue;

      var posicao = custodia.FirstOrDefault(c => c.Ticker == item.Ticker);
      if (posicao == null)
      {
        posicao = new Custodia(cliente.ContaGrafica.Id);
        posicao.AtualizarCustodia(quantidade, cotacao.PrecoFechamento, item.Ticker);
        await _custodiaRepository.AddAsync(posicao);
      }
      else
      {
        var quantidadeAnterior = posicao.Quantidade;
        var valorAnterior = quantidadeAnterior * posicao.PrecoMedio;
        var valorNovo = quantidade * cotacao.PrecoFechamento;
        var quantidadeTotal = quantidadeAnterior + quantidade;
        var precoMedioNovo = (valorAnterior + valorNovo) / quantidadeTotal;

        posicao.AtualizarCustodia(quantidadeTotal, precoMedioNovo);
      }
    }
  }

  private async Task RegistrarVendaParaIR(Cliente cliente, string ticker, int quantidade, decimal precoVenda, decimal precoMedio)
  {
    var valorVenda = quantidade * precoVenda;
    var valorCusto = quantidade * precoMedio;
    var lucro = valorVenda - valorCusto;

    // Acumular vendas do mês para cálculo de IR
    var vendasMes = await ObterVendasMes(cliente.Id, DateTime.UtcNow);
    var totalVendasMes = vendasMes.Sum(v => v.ValorVenda) + valorVenda;

    if (totalVendasMes > 20000) // Isenção de R$ 20.000
    {
      var valorIR = lucro * 0.20m; // 20% sobre lucro

      var irEvent = new
      {
        Tipo = "IrVenda",
        ClienteId = cliente.Id,
        Ticker = ticker,
        Quantidade = quantidade,
        ValorVenda = valorVenda,
        CustoAquisicao = valorCusto,
        Lucro = lucro,
        IrRetido = valorIR,
        DataOperacao = DateTime.UtcNow,
        PercentualIR = 0.20m
      };

      await _messagePublisher.PublishAsync("ir-events", System.Text.Json.JsonSerializer.Serialize(irEvent));
    }
  }

  private Task<List<Venda>> ObterVendasMes(long clienteId, DateTime data)
  {
    // Implementação simplificada - deveria buscar do repositório de vendas
    return Task.FromResult(new List<Venda>());
  }

  private async Task ExecutarOperacoesRebalanceamento(Cliente cliente, List<object> operacoes)
  {
    var custodia = await _custodiaRepository.ObterPorContaGraficaAsync(cliente.ContaGrafica.Id);

    foreach (var operacao in operacoes)
    {
      var tipo = operacao.GetType().GetProperty("Tipo")?.GetValue(operacao)?.ToString();
      var ticker = operacao.GetType().GetProperty("Ticker")?.GetValue(operacao)?.ToString();
      var quantidade = (int)operacao.GetType().GetProperty("Quantidade")?.GetValue(operacao)!;
      var preco = (decimal)operacao.GetType().GetProperty("Preco")?.GetValue(operacao)!;

      var posicao = custodia.FirstOrDefault(c => c.Ticker == ticker);

      if (tipo == "Venda" && posicao != null)
      {
        posicao.AtualizarQuantidade(posicao.Quantidade - quantidade);
      }
      else if (tipo == "Compra")
      {
        if (posicao == null)
        {
          posicao = new Custodia(cliente.ContaGrafica.Id);
          posicao.AtualizarCustodia(quantidade, preco, ticker);
          await _custodiaRepository.AddAsync(posicao);
        }
        else
        {
          var quantidadeAnterior = posicao.Quantidade;
          var valorAnterior = quantidadeAnterior * posicao.PrecoMedio;
          var valorNovo = quantidade * preco;
          var quantidadeTotal = quantidadeAnterior + quantidade;
          var precoMedioNovo = (valorAnterior + valorNovo) / quantidadeTotal;

          posicao.AtualizarCustodia(quantidadeTotal, precoMedioNovo);
        }
      }
    }
  }
}
