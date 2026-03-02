namespace CompraProgramadaAcoes.Application.DTOs;

public class SaidaResponse
{
    public long ClienteId { get; set; }
    public string Nome { get; set; } = null!;
    public bool Ativo { get; set; }
    public string DataSaida { get; set; } = default!;
    public string Mensagem { get; set; } = null!;
}
