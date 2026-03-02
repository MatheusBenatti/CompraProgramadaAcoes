namespace CompraProgramadaAcoes.Domain.Entities;

public class EventoIR
{
    public long Id { get; private set; }
    public long ClienteId { get; private set; }
    public TipoEventoIR Tipo { get; private set; }
    public decimal ValorBase { get; private set; }
    public decimal ValorIR { get; private set; }
    public bool PublicadoKafka { get; private set; } = false;
    public DateTime DataEvento { get; private set; } = DateTime.UtcNow;

    public Cliente Cliente { get; private set; } = null!;

    public EventoIR(long clienteId, TipoEventoIR tipo, decimal valorBase, decimal valorIR)
    {
        ClienteId = clienteId;
        Tipo = tipo;
        ValorBase = valorBase;
        ValorIR = valorIR;
    }

    private EventoIR() { } // Para EF Core

    public void MarcarComoPublicado()
    {
        PublicadoKafka = true;
    }
}

public enum TipoEventoIR
{
    DedoDuro,
    IrVenda
}
