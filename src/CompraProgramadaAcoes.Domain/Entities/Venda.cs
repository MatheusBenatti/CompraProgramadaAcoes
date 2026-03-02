namespace CompraProgramadaAcoes.Domain.Entities;

public class Venda
{
    public long Id { get; private set; }
    public long ClienteId { get; private set; }
    public string Ticker { get; private set; } = string.Empty;
    public int Quantidade { get; private set; }
    public decimal ValorVenda { get; private set; }
    public decimal PrecoUnitario { get; private set; }
    public DateTime DataOperacao { get; private set; }
    public decimal CustoAquisicao { get; private set; }
    public decimal Lucro { get; private set; }
    public decimal IrRetido { get; private set; }
    
    public Venda(long clienteId, string ticker, int quantidade, decimal precoUnitario, decimal custoAquisicao)
    {
        ClienteId = clienteId;
        Ticker = ticker;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
        ValorVenda = quantidade * precoUnitario;
        CustoAquisicao = custoAquisicao;
        Lucro = ValorVenda - CustoAquisicao;
        DataOperacao = DateTime.UtcNow;
        IrRetido = 0;
    }
    
    public void CalcularIR(decimal percentualIR)
    {
        IrRetido = Lucro * percentualIR;
    }
}
