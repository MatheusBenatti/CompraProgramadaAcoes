namespace CompraProgramadaAcoes.Application.DTOs.Motor;

public class ExecucaoCompraResponse
{
  public DateTime DataExecucao { get; set; }
  public int TotalClientes { get; set; }
  public decimal TotalConsolidado { get; set; }
  public List<OrdemCompraResponse> OrdensCompra { get; set; } = new();
  public List<DistribuicaoResponse> Distribuicoes { get; set; } = new();
  public List<ResiduoResponse> ResiduosCustMaster { get; set; } = new();
  public int EventosIRPublicados { get; set; }
  public string Mensagem { get; set; } = string.Empty;
}

public class OrdemCompraResponse
{
  public string Ticker { get; set; } = string.Empty;
  public int QuantidadeTotal { get; set; }
  public List<OrdemCompraDetalheResponse> Detalhes { get; set; } = new();
  public decimal PrecoUnitario { get; set; }
  public decimal ValorTotal { get; set; }
}

public class OrdemCompraDetalheResponse
{
  public string Tipo { get; set; } = string.Empty;
  public string Ticker { get; set; } = string.Empty;
  public int Quantidade { get; set; }
}

public class DistribuicaoResponse
{
  public long ClienteId { get; set; }
  public string Nome { get; set; } = string.Empty;
  public decimal ValorAporte { get; set; }
  public List<AtivoDistribuidoResponse> Ativos { get; set; } = new();
}

public class AtivoDistribuidoResponse
{
  public string Ticker { get; set; } = string.Empty;
  public int Quantidade { get; set; }
}

public class ResiduoResponse
{
  public string Ticker { get; set; } = string.Empty;
  public int Quantidade { get; set; }
}
