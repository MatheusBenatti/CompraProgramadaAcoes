namespace CompraProgramadaAcoes.Domain.Entities;

public class Cotacao
{
    public long Id { get; private set; }
    public DateTime DataPregao { get; private set; }
    public string Ticker { get; private set; } = string.Empty;
    public decimal PrecoAbertura { get; private set; }
    public decimal PrecoFechamento { get; private set; }
    public decimal PrecoMaximo { get; private set; }
    public decimal PrecoMinimo { get; private set; }

    public Cotacao(DateTime dataPregao, string ticker, decimal precoAbertura, decimal precoFechamento, decimal precoMaximo, decimal precoMinimo)
    {
        DataPregao = dataPregao;
        Ticker = ticker;
        PrecoAbertura = precoAbertura;
        PrecoFechamento = precoFechamento;
        PrecoMaximo = precoMaximo;
        PrecoMinimo = precoMinimo;
    }

    private Cotacao() { } // Para EF Core
}
