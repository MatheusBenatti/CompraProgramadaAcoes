using CompraProgramadaAcoes.Domain.Entities;
using System.Text;
using System.Globalization;

namespace CompraProgramadaAcoes.Application.Services;

public class CotahistParser
{
    /// <summary>
    /// Lê e faz parse de um arquivo COTAHIST da B3.
    /// Retorna apenas registros de detalhe (TIPREG = 01)
    /// filtrados por mercado a vista (010) e fracionário (020).
    /// </summary>
    public virtual IEnumerable<CotacaoB3> ParseArquivo(string caminhoArquivo)
    {
        var cotacoes = new List<CotacaoB3>();
        
        // Registrar encoding ISO-8859-1 para ler o arquivo corretamente
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var encoding = Encoding.GetEncoding("ISO-8859-1");
        
        foreach (var linha in File.ReadLines(caminhoArquivo, encoding))
        {
            // Ignorar header (00) e trailer (99)
            if (linha.Length < 245)
                continue;
                
            var tipoRegistro = linha.Substring(0, 2);
            if (tipoRegistro != "01")
                continue;

            var tipoMercadoStr = linha.Substring(24, 3).Trim();
            if (!int.TryParse(tipoMercadoStr, out var tipoMercado))
                continue;
            
            // Filtrar apenas mercado a vista (010) e fracionário (020)
            if (tipoMercado != 10 && tipoMercado != 20)
                continue;

            var cotacao = new CotacaoB3
            {
                DataPregao = DateTime.ParseExact(
                    linha.Substring(2, 8), "yyyyMMdd", 
                    CultureInfo.InvariantCulture),
                CodigoBDI = linha.Substring(10, 2).Trim(),
                Ticker = linha.Substring(12, 12).Trim(),
                TipoMercado = tipoMercado,
                NomeEmpresa = linha.Substring(27, 12).Trim(),
                PrecoAbertura = ParsePreco(linha.Substring(56, 13)),
                PrecoMaximo = ParsePreco(linha.Substring(69, 13)),
                PrecoMinimo = ParsePreco(linha.Substring(82, 13)),
                PrecoMedio = ParsePreco(linha.Substring(95, 13)),
                PrecoFechamento = ParsePreco(linha.Substring(108, 13)),
                QuantidadeNegociada = long.Parse(linha.Substring(152, 18).Trim()),
                VolumeNegociado = ParsePreco(linha.Substring(170, 18))
            };

            cotacoes.Add(cotacao);
        }

        return cotacoes;
    }

    /// <summary>
    /// Converte o valor inteiro do arquivo para decimal com 2 casas.
    /// Ex: "0000000003850" => 38.50m
    /// </summary>
    private decimal ParsePreco(string valorBruto)
    {
        if (long.TryParse(valorBruto.Trim(), out var valor))
            return valor / 100m;
        return 0m;
    }

    /// <summary>
    /// Obtém a cotação de fechamento mais recente de um ticker específico.
    /// Busca na pasta cotacoes/ o arquivo mais recente.
    /// </summary>
    public virtual CotacaoB3? ObterCotacaoFechamento(string pastaCotacoes, string ticker)
    {
        var arquivos = Directory.GetFiles(pastaCotacoes, "COTAHIST_D*.TXT")
            .OrderByDescending(f => f)
            .ToList();

        foreach (var arquivo in arquivos)
        {
            var cotacoes = ParseArquivo(arquivo);
            var cotacao = cotacoes
                .Where(c => c.Ticker.Equals(ticker, StringComparison.OrdinalIgnoreCase))
                .Where(c => c.TipoMercado == 10) // Mercado a vista
                .FirstOrDefault();

            if (cotacao != null)
                return cotacao;
        }

        return null;
    }

    /// <summary>
    /// Obtém cotações de fechamento para múltiplos tickers.
    /// </summary>
    public virtual Dictionary<string, CotacaoB3> ObterCotacoesFechamento(string pastaCotacoes, IEnumerable<string> tickers)
    {
        var resultado = new Dictionary<string, CotacaoB3>();
        
        foreach (var ticker in tickers)
        {
            var cotacao = ObterCotacaoFechamento(pastaCotacoes, ticker);
            if (cotacao != null)
                resultado[ticker] = cotacao;
        }
        
        return resultado;
    }
}
