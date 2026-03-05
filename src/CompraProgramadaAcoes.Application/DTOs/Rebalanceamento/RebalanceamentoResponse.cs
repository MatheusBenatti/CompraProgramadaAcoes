namespace CompraProgramadaAcoes.Application.DTOs.Rebalanceamento;

public class RebalanceamentoResponse
{
    public DateTime DataExecucao { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public bool Sucesso { get; set; }
    public int TotalClientesAfetados { get; set; }
    public List<string> AtivosRemovidos { get; set; } = new();
    public List<string> AtivosAdicionados { get; set; } = new();
}
