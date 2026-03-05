namespace CompraProgramadaAcoes.Application.DTOs;

public class RentabilidadeResponse
{
    public long ClienteId { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public decimal SaldoTotal { get; set; }
    public decimal PLTotal { get; set; }
    public decimal RentabilidadePercentual { get; set; }
    public List<AtivoRentabilidadeResponse> Ativos { get; set; } = new();
    public List<EvolucaoRentabilidadeResponse> HistoricoEvolucao { get; set; } = new();
    public DateTime DataReferencia { get; set; } = DateTime.UtcNow;
}

public class AtivoRentabilidadeResponse
{
    public string Ticker { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoMedio { get; set; }
    public decimal ValorAtual { get; set; }
    public decimal PL { get; set; }
    public decimal RentabilidadePercentual { get; set; }
    public decimal PesoCarteira { get; set; }
}

public class EvolucaoRentabilidadeResponse
{
    public DateTime Data { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal Rentabilidade { get; set; }
    public decimal PLAcumulado { get; set; }
}
