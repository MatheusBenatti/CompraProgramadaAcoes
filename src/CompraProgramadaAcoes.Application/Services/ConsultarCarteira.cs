using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Application.Exceptions;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Application.Interfaces.UseCases;
using Microsoft.Extensions.Logging;

namespace CompraProgramadaAcoes.Application.Services;

public class ConsultarCarteira(
    IClienteRepository clienteRepository,
    ICustodiaRepository custodiaRepository,
    ICotacaoRepository cotacaoRepository,
    CotahistParser cotahistParser,
    ILogger<ConsultarCarteira> logger,
    string pastaCotacoes = "cotacoes") : IConsultarCarteira
{
  private readonly IClienteRepository _clienteRepository = clienteRepository;
  private readonly ICustodiaRepository _custodiaRepository = custodiaRepository;
  private readonly ICotacaoRepository _cotacaoRepository = cotacaoRepository;
  private readonly CotahistParser _cotahistParser = cotahistParser;
  private readonly ILogger<ConsultarCarteira> _logger = logger;
  private readonly string _pastaCotacoes = pastaCotacoes;

  public async Task<CarteiraResponse> ExecuteAsync(long clienteId)
  {
    try
    {
      var cliente = await _clienteRepository.GetByIdAsync(clienteId) ?? throw new ClienteNaoEncontradoException();
      var custodia = await _custodiaRepository.ObterPorContaGraficaAsync(cliente.ContaGrafica.Id);

      if (custodia.Count == 0)
      {
        return new CarteiraResponse
        {
          ClienteId = clienteId,
          NomeCliente = cliente.Nome,
          ValorTotal = 0,
          CustoTotal = 0,
          LucroPrejuizoTotal = 0,
          RentabilidadePercentual = 0
        };
      }

      // Obter cotações atuais dos ativos
      var tickers = custodia.Select(c => c.Ticker);
      var cotacoes = _cotahistParser.ObterCotacoesFechamento(_pastaCotacoes, tickers);

      var ativosResponse = new List<AtivoCarteiraResponse>();
      decimal valorTotal = 0;
      decimal custoTotal = 0;

      foreach (var posicao in custodia.Where(c => c.Quantidade > 0))
      {
        var precoAtual = cotacoes.TryGetValue(posicao.Ticker!, out var cotacao) ? cotacao.PrecoFechamento : posicao.PrecoMedio;
        var valorPosicao = posicao.Quantidade * precoAtual;
        var custoPosicao = posicao.Quantidade * posicao.PrecoMedio;
        var lucroPrejuizo = valorPosicao - custoPosicao;
        var rentabilidadePercentual = custoPosicao > 0 ? (lucroPrejuizo / custoPosicao) * 100 : 0;

        ativosResponse.Add(new AtivoCarteiraResponse
        {
          Ticker = posicao.Ticker!,
          Quantidade = posicao.Quantidade,
          PrecoMedio = posicao.PrecoMedio,
          PrecoAtual = precoAtual,
          ValorTotal = valorPosicao,
          CustoTotal = custoPosicao,
          LucroPrejuizo = lucroPrejuizo,
          RentabilidadePercentual = rentabilidadePercentual
        });

        valorTotal += valorPosicao;
        custoTotal += custoPosicao;
      }

      var lucroPrejuizoTotal = valorTotal - custoTotal;
      var rentabilidadePercentualTotal = custoTotal > 0 ? (lucroPrejuizoTotal / custoTotal) * 100 : 0;

      return new CarteiraResponse
      {
        ClienteId = clienteId,
        NomeCliente = cliente.Nome,
        Ativos = ativosResponse,
        ValorTotal = valorTotal,
        CustoTotal = custoTotal,
        LucroPrejuizoTotal = lucroPrejuizoTotal,
        RentabilidadePercentual = rentabilidadePercentualTotal
      };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Erro ao consultar carteira do cliente {ClienteId}", clienteId);
      throw;
    }
  }
}
