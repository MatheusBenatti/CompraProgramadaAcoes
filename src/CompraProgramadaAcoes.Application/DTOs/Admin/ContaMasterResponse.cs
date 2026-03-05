namespace CompraProgramadaAcoes.Application.DTOs.Admin;

public class ContaMasterCustodiaResponse
{
    public ContaMasterInfoResponse ContaMaster { get; set; } = null!;
    public List<ContaMasterCustodiaItemResponse> Custodia { get; set; } = new();
    public decimal ValorTotalResiduo { get; set; }
}

public class ContaMasterInfoResponse
{
    public long Id { get; set; }
    public string NumeroConta { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
}

public class ContaMasterCustodiaItemResponse
{
    public string Ticker { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoMedio { get; set; }
    public decimal ValorAtual { get; set; }
    public string Origem { get; set; } = string.Empty;
}
