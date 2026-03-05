namespace CompraProgramadaAcoes.Application.DTOs.Admin;

public class CestaAtualResponse
{
    public long CestaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativa { get; set; }
    public DateTime DataCriacao { get; set; }
    public List<CestaItemAtualResponse> Itens { get; set; } = new();
}

public class CestaItemAtualResponse
{
    public string Ticker { get; set; } = string.Empty;
    public decimal Percentual { get; set; }
    public decimal CotacaoAtual { get; set; }
}

public class CestasHistoricoResponse
{
    public List<CestaHistoricoItemResponse> Cestas { get; set; } = new();
}

public class CestaHistoricoItemResponse
{
    public long CestaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativa { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataDesativacao { get; set; }
    public List<CestaItemAdminResponse> Itens { get; set; } = new();
}
