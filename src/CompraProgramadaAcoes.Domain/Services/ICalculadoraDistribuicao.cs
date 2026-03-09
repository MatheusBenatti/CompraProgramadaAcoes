using CompraProgramadaAcoes.Domain.Entities;
using CompraProgramadaAcoes.Domain.ValueObjects;

namespace CompraProgramadaAcoes.Domain.Services;

public interface ICalculadoraDistribuicao
{
  DistribuicaoResult CalcularDistribuicao(
      ValorMonetario valorTotal,
      List<ClienteAggregate> clientes,
      CestaRecomendacao cesta,
      Dictionary<Ticker, decimal> cotacoes
  );

  CompraConsolidadaResult CalcularCompraConsolidada(
      Dictionary<long, ValorMonetario> aportesClientes,
      CestaRecomendacao cesta,
      Dictionary<Ticker, decimal> cotacoes
  );
}

public record DistribuicaoResult(
    Dictionary<long, Dictionary<Ticker, int>> DistribuicaoPorCliente,
    Dictionary<Ticker, int> ResiduosMaster
);

public record CompraConsolidadaResult(
    Dictionary<Ticker, (int Quantidade, decimal Valor)> ComprasPorAtivo,
    ValorMonetario ValorTotal,
    Dictionary<Ticker, int> SaldoDisponivelMaster
);
