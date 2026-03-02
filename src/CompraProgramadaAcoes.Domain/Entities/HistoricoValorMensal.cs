namespace CompraProgramadaAcoes.Domain.Entities;

public class HistoricoValorMensal(long clienteId, decimal valorAnterior, decimal valorNovo)
{
    public long Id { get; private set; }
    public long ClienteId { get; private set; } = clienteId;
    public decimal ValorAnterior { get; private set; } = valorAnterior;
    public decimal ValorNovo { get; private set; } = valorNovo;
    public DateTime DataAlteracao { get; private set; } = DateTime.UtcNow;
}
