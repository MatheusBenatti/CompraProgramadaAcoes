namespace CompraProgramadaAcoes.Application.DTOs.Admin;

public class CestaAdminResponse
{
    public long CestaId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativa { get; set; }
    public DateTime DataCriacao { get; set; }
    public List<CestaItemAdminResponse> Itens { get; set; } = new();
    public bool RebalanceamentoDisparado { get; set; }
    public string Mensagem { get; set; } = string.Empty;
}

public class CestaItemAdminResponse
{
    public string Ticker { get; set; } = string.Empty;
    public decimal Percentual { get; set; }
}

public class CadastrarCestaAdminRequest
{
    public string Nome { get; set; } = string.Empty;
    public List<CadastrarCestaItemRequest> Itens { get; set; } = new();
}

public class CadastrarCestaItemRequest
{
    public string Ticker { get; set; } = string.Empty;
    public decimal Percentual { get; set; }
}
