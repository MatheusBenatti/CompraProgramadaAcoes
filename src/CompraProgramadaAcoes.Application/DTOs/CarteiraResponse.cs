namespace CompraProgramadaAcoes.Application.DTOs;

public class CarteiraResponse
{
    public long ClienteId { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public List<AtivoCarteiraResponse> Ativos { get; set; } = new();
    public decimal ValorTotal { get; set; }
    public decimal CustoTotal { get; set; }
    public decimal LucroPrejuizoTotal { get; set; }
    public decimal RentabilidadePercentual { get; set; }
    public DateTime DataConsulta { get; set; } = DateTime.UtcNow;
}

public class AtivoCarteiraResponse
{
    public string Ticker { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoMedio { get; set; }
    public decimal PrecoAtual { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal CustoTotal { get; set; }
    public decimal LucroPrejuizo { get; set; }
    public decimal RentabilidadePercentual { get; set; }
}
