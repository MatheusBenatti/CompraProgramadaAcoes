namespace CompraProgramadaAcoes.Application.DTOs;

public class CestaCacheDTO
{
    public string Nome { get; set; } = "Top Five";
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public bool Ativa { get; set; } = true;
    public List<ItemCestaCacheDTO> Itens { get; set; } = new();
    
    public bool IsValida()
    {
        return Ativa && 
               Math.Abs(Itens.Sum(i => i.Percentual) - 100) < 0.01m && 
               Itens.Count == 5;
    }
}

public class ItemCestaCacheDTO
{
    public string Ticker { get; set; } = string.Empty;
    public decimal Percentual { get; set; }
}
