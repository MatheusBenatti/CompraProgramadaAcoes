namespace CompraProgramadaAcoes.Application.DTOs;

public class CotacaoCacheDTO
{
    public string Ticker { get; set; } = string.Empty;
    public DateTime DataPregao { get; set; }
    public decimal PrecoAbertura { get; set; }
    public decimal PrecoFechamento { get; set; }
    public decimal PrecoMaximo { get; set; }
    public decimal PrecoMinimo { get; set; }
    public decimal PrecoMedio { get; set; }
    public long QuantidadeNegociada { get; set; }
    public decimal VolumeNegociado { get; set; }
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
}

public class CotacoesCestaDTO
{
    public DateTime DataReferencia { get; set; } = DateTime.UtcNow;
    public Dictionary<string, CotacaoCacheDTO> Cotacoes { get; set; } = new();
    
    public CotacaoCacheDTO? ObterCotacao(string ticker)
    {
        return Cotacoes.TryGetValue(ticker.ToUpper(), out var cotacao) ? cotacao : null;
    }
    
    public void AdicionarCotacao(CotacaoCacheDTO cotacao)
    {
        Cotacoes[cotacao.Ticker.ToUpper()] = cotacao;
    }
}
