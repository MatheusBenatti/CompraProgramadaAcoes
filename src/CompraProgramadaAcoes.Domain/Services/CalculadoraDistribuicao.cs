using CompraProgramadaAcoes.Domain.Entities;
using CompraProgramadaAcoes.Domain.ValueObjects;

namespace CompraProgramadaAcoes.Domain.Services;

public class CalculadoraDistribuicao : ICalculadoraDistribuicao
{
    public DistribuicaoResult CalcularDistribuicao(
        ValorMonetario valorTotal,
        List<ClienteAggregate> clientes,
        CestaRecomendacao cesta,
        Dictionary<Ticker, decimal> cotacoes
    ){
        var distribuicaoPorCliente = new Dictionary<Guid, Dictionary<Ticker, int>>();
        var residuosMaster = new Dictionary<Ticker, int>();

        // Calcular valor proporcional de cada cliente
        var valorPorCliente = clientes.ToDictionary(
            c => c.Id,
            c => valorTotal.Multiplicar((decimal)c.ValorMensal / clientes.Sum(x => x.ValorMensal))
        );

        // Para cada ativo da cesta
        foreach (var itemCesta in cesta.Itens)
        {
            var valorTotalAtivo = valorTotal.Multiplicar(itemCesta.Percentual / 100);
            var cotacao = cotacoes[itemCesta.Ticker];
            var totalQuantidadeAtivo = (int)Math.Floor(valorTotalAtivo / cotacao);
            
            var quantidadeDistribuida = 0;

            // Distribuir proporcionalmente entre clientes
            foreach (var cliente in clientes.OrderByDescending(c => c.ValorMensal))
            {
                var valorCliente = valorPorCliente[cliente.Id];
                var valorAtivoCliente = valorTotalAtivo.Multiplicar((decimal)cliente.ValorMensal / clientes.Sum(x => x.ValorMensal));
                var quantidadeCliente = (int)Math.Floor(valorAtivoCliente / cotacao);

                if (quantidadeCliente > 0)
                {
                    if (!distribuicaoPorCliente.ContainsKey(cliente.Id))
                        distribuicaoPorCliente[cliente.Id] = new Dictionary<Ticker, int>();

                    distribuicaoPorCliente[cliente.Id][itemCesta.Ticker] = quantidadeCliente;
                    quantidadeDistribuida += quantidadeCliente;
                }
            }

            // Resíduo fica na master
            var residuo = totalQuantidadeAtivo - quantidadeDistribuida;
            if (residuo > 0)
            {
                residuosMaster[itemCesta.Ticker] = residuo;
            }
        }

        return new DistribuicaoResult(distribuicaoPorCliente, residuosMaster);
    }

    public CompraConsolidadaResult CalcularCompraConsolidada(
        Dictionary<Guid, ValorMonetario> aportesClientes,
        CestaRecomendacao cesta,
        Dictionary<Ticker, decimal> cotacoes)
    {
        var valorTotal = aportesClientes.Values.Aggregate((a, b) => a.Somar(b));
        var comprasPorAtivo = new Dictionary<Ticker, (int Quantidade, decimal Valor)>();

        foreach (var itemCesta in cesta.Itens)
        {
            var valorAtivo = valorTotal.Multiplicar(itemCesta.Percentual / 100);
            var cotacao = cotacoes[itemCesta.Ticker];
            var quantidade = (int)Math.Floor(valorAtivo / cotacao);
            var valorReal = quantidade * cotacao;

            comprasPorAtivo[itemCesta.Ticker] = (quantidade, valorReal);
        }

        return new CompraConsolidadaResult(comprasPorAtivo, valorTotal, new Dictionary<Ticker, int>());
    }

    // Método auxiliar - deveria vir de um serviço externo
}
