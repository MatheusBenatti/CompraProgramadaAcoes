namespace CompraProgramadaAcoes.Domain.Entities;

public class OrdemCompra
{
    public long Id { get; private set; }
    public long ContaMasterId { get; private set; }
    public string Ticker { get; private set; } = string.Empty;
    public int Quantidade { get; private set; }
    public decimal PrecoUnitario { get; private set; } = 0;
    public TipoMercado TipoMercado { get; private set; }
    public DateTime DataExecucao { get; private set; } = DateTime.UtcNow;

    public ContaGrafica ContaMaster { get; private set; } = null!;
    public ICollection<Distribuicao> Distribuicoes { get; private set; } = new List<Distribuicao>();

    public OrdemCompra(long contaMasterId, string ticker, int quantidade, decimal precoUnitario, TipoMercado tipoMercado)
    {
        ContaMasterId = contaMasterId;
        Ticker = ticker;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
        TipoMercado = tipoMercado;
    }

    public void AtualizarPrecoUnitario(decimal precoUnitario)
    {
        PrecoUnitario = precoUnitario;
    }

    public void AtualizarTipoMercado(TipoMercado tipoMercado)
    {
        TipoMercado = tipoMercado;
    }

    public void AtualizarDataExecucao(DateTime dataExecucao)
    {
        DataExecucao = dataExecucao;
    }

    private OrdemCompra() { } // Para EF Core
}

public enum TipoMercado
{
    Lote,
    Fracionario
}
