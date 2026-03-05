using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Application.Exceptions;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Application.Interfaces.UseCases;
using Microsoft.Extensions.Logging;

namespace CompraProgramadaAcoes.Application.Services;

public class ConsultarRentabilidade : IConsultarRentabilidade
{
    private readonly IClienteRepository _clienteRepository;
    private readonly ICustodiaRepository _custodiaRepository;
    private readonly IDistribuicaoRepository _distribuicaoRepository;
    private readonly CotahistParser _cotahistParser;
    private readonly ILogger<ConsultarRentabilidade> _logger;
    private readonly string _pastaCotacoes;

    public ConsultarRentabilidade(
        IClienteRepository clienteRepository,
        ICustodiaRepository custodiaRepository,
        IDistribuicaoRepository distribuicaoRepository,
        CotahistParser cotahistParser,
        ILogger<ConsultarRentabilidade> logger,
        string pastaCotacoes = "cotacoes")
    {
        _clienteRepository = clienteRepository;
        _custodiaRepository = custodiaRepository;
        _distribuicaoRepository = distribuicaoRepository;
        _cotahistParser = cotahistParser;
        _logger = logger;
        _pastaCotacoes = pastaCotacoes;
    }

    public async Task<RentabilidadeResponse> ExecuteAsync(long clienteId)
    {
        try
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
            {
                throw new ClienteNaoEncontradoException();
            }

            var custodia = await _custodiaRepository.ObterPorContaGraficaAsync(cliente.ContaGrafica.Id);
            
            if (!custodia.Any())
            {
                return new RentabilidadeResponse
                {
                    ClienteId = clienteId,
                    NomeCliente = cliente.Nome,
                    SaldoTotal = 0,
                    PLTotal = 0,
                    RentabilidadePercentual = 0
                };
            }

            // Obter cotações atuais
            var tickers = custodia.Select(c => c.Ticker);
            var cotacoes = _cotahistParser.ObterCotacoesFechamento(_pastaCotacoes, tickers);

            // Calcular valores atuais
            var ativosResponse = new List<AtivoRentabilidadeResponse>();
            decimal valorTotal = 0;
            decimal custoTotal = 0;
            decimal plTotal = 0;

            foreach (var posicao in custodia.Where(c => c.Quantidade > 0))
            {
                var precoAtual = cotacoes.TryGetValue(posicao.Ticker, out var cotacao) ? cotacao.PrecoFechamento : posicao.PrecoMedio;
                var valorPosicao = posicao.Quantidade * precoAtual;
                var custoPosicao = posicao.Quantidade * posicao.PrecoMedio;
                var plAtivo = valorPosicao - custoPosicao;
                var rentabilidadePercentual = custoPosicao > 0 ? (plAtivo / custoPosicao) * 100 : 0;

                ativosResponse.Add(new AtivoRentabilidadeResponse
                {
                    Ticker = posicao.Ticker,
                    Quantidade = posicao.Quantidade,
                    PrecoMedio = posicao.PrecoMedio,
                    ValorAtual = valorPosicao,
                    PL = plAtivo,
                    RentabilidadePercentual = rentabilidadePercentual
                });

                valorTotal += valorPosicao;
                custoTotal += custoPosicao;
                plTotal += plAtivo;
            }

            // Calcular peso de cada ativo na carteira
            foreach (var ativo in ativosResponse)
            {
                ativo.PesoCarteira = valorTotal > 0 ? (ativo.ValorAtual / valorTotal) * 100 : 0;
            }

            var rentabilidadePercentualTotal = custoTotal > 0 ? (plTotal / custoTotal) * 100 : 0;

            // Obter histórico de evolução (últimos 12 meses)
            var historico = await ObterHistoricoEvolucaoAsync(clienteId);

            return new RentabilidadeResponse
            {
                ClienteId = clienteId,
                NomeCliente = cliente.Nome,
                SaldoTotal = valorTotal,
                PLTotal = plTotal,
                RentabilidadePercentual = rentabilidadePercentualTotal,
                Ativos = ativosResponse,
                HistoricoEvolucao = historico
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar rentabilidade do cliente {ClienteId}", clienteId);
            throw;
        }
    }

    private async Task<List<EvolucaoRentabilidadeResponse>> ObterHistoricoEvolucaoAsync(long clienteId)
    {
        var historico = new List<EvolucaoRentabilidadeResponse>();
        
        // Obter histórico de distribuições para calcular evolução
        var distribuicoes = await _distribuicaoRepository.ObterPorClienteAsync(clienteId);
        
        // Agrupar por mês para calcular evolução
        var evolucaoMensal = distribuicoes
            .GroupBy(d => new { d.DataDistribuicao.Year, d.DataDistribuicao.Month })
            .Select(g => new
            {
                Ano = g.Key.Year,
                Mes = g.Key.Month,
                ValorTotal = g.Sum(d => d.Quantidade * d.PrecoUnitario),
                CustoTotal = g.Sum(d => d.Quantidade * d.PrecoUnitario)
            })
            .OrderBy(x => x.Ano).ThenBy(x => x.Mes)
            .ToList();

        decimal plAcumulado = 0;
        
        foreach (var mes in evolucaoMensal)
        {
            plAcumulado += (mes.ValorTotal - mes.CustoTotal);
            var rentabilidadeMes = mes.CustoTotal > 0 ? ((mes.ValorTotal - mes.CustoTotal) / mes.CustoTotal) * 100 : 0;
            
            historico.Add(new EvolucaoRentabilidadeResponse
            {
                Data = new DateTime(mes.Ano, mes.Mes, 1, 0, 0, 0, DateTimeKind.Utc),
                ValorTotal = mes.ValorTotal,
                Rentabilidade = rentabilidadeMes,
                PLAcumulado = plAcumulado
            });
        }

        return historico;
    }
}
