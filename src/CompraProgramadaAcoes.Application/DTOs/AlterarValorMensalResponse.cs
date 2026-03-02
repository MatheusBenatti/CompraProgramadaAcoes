namespace CompraProgramadaAcoes.Application.DTOs;

public class AlterarValorMensalResponse
{
    public long ClienteId { get; set; }
    public decimal ValorMensalAnterior { get; set; }
    public decimal ValorMensalNovo { get; set; }
    public string DataAlteracao { get; set; } = default!;
    public string Mensagem { get; set; } = null!;
}
