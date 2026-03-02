namespace CompraProgramadaAcoes.Domain.Entities;

public class Distribuicao
{
    public long Id { get; private set; }
    public long OrdemCompraId { get; private set; }
    public long CustodiaFilhoteId { get; private set; }
    public string Ticker { get; private set; } = string.Empty;
    public int Quantidade { get; private set; }
    public decimal PrecoUnitario { get; private set; } = 0;
    public DateTime DataDistribuicao { get; private set; } = DateTime.UtcNow;

    public OrdemCompra OrdemCompra { get; private set; } = null!;
    public Custodia CustodiaFilhote { get; private set; } = null!;

    public Distribuicao(long ordemCompraId, long custodiaFilhoteId, string ticker, int quantidade, decimal precoUnitario)
    {
        OrdemCompraId = ordemCompraId;
        CustodiaFilhoteId = custodiaFilhoteId;
        Ticker = ticker;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
    }

    public void AtualizarOrdemCompraId(long ordemCompraId)
    {
        OrdemCompraId = ordemCompraId;
    }

    public void AtualizarTicker(string ticker)
    {
        Ticker = ticker;
    }

    public void AtualizarQuantidade(int quantidade)
    {
        Quantidade = quantidade;
    }

    public void AtualizarPrecoUnitario(decimal precoUnitario)
    {
        PrecoUnitario = precoUnitario;
    }

    public void AtualizarDataDistribuicao(DateTime dataDistribuicao)
    {
        DataDistribuicao = dataDistribuicao;
    }

    private Distribuicao() { } // Para EF Core
}
