namespace CompraProgramadaAcoes.Domain.Entities;

public class CestaRecomendacao
{
  public long Id { get; private set; }
  public string Nome { get; private set; } = "Top Five";
  public bool Ativa { get; private set; } = true;
  public DateTime DataCriacao { get; private set; } = DateTime.UtcNow;
  public DateTime? DataDesativacao { get; private set; }
  public ICollection<ItemCesta> Itens { get; private set; } = new List<ItemCesta>();

  public void AdicionarItem(string ticker, decimal percentual)
  {
    if (Itens.Any(i => i.Ticker.Equals(ticker, StringComparison.OrdinalIgnoreCase)))
      throw new InvalidOperationException($"Ticker {ticker} já existe na cesta");

    var totalPercentual = Itens.Sum(i => i.Percentual) + percentual;
    if (totalPercentual > 100)
      throw new InvalidOperationException("Soma dos percentuais não pode ultrapassar 100%");

    Itens.Add(new ItemCesta(ticker, percentual));
  }

  public void AtualizarNome(string nome)
  {
    Nome = nome;
  }

  public void RemoverItem(string ticker)
  {
    var item = Itens.FirstOrDefault(i => i.Ticker.Equals(ticker, StringComparison.OrdinalIgnoreCase));
    if (item != null)
      Itens.Remove(item);
  }

  public void AtualizarPercentual(string ticker, decimal novoPercentual)
  {
    var item = Itens.FirstOrDefault(i => i.Ticker.Equals(ticker, StringComparison.OrdinalIgnoreCase));
    if (item == null)
      throw new InvalidOperationException($"Ticker {ticker} não encontrado na cesta");

    var outrosItens = Itens.Where(i => !i.Ticker.Equals(ticker, StringComparison.OrdinalIgnoreCase)).ToList();
    var totalOutros = outrosItens.Sum(i => i.Percentual);

    if (totalOutros + novoPercentual > 100)
      throw new InvalidOperationException("Soma dos percentuais não pode ultrapassar 100%");

    item.AtualizarPercentual(novoPercentual);
  }

  public void Desativar()
  {
    Ativa = false;
    DataDesativacao = DateTime.UtcNow;
  }

  public bool IsValida()
  {
    return Ativa && Math.Abs(Itens.Sum(i => i.Percentual) - 100) < 0.01m && Itens.Count == 5;
  }
}

public class ItemCesta
{
  public long Id { get; private set; }
  public long CestaId { get; private set; }
  public string Ticker { get; private set; } = string.Empty;
  public decimal Percentual { get; private set; }

  public CestaRecomendacao Cesta { get; private set; } = null!;

  public ItemCesta(string ticker, decimal percentual)
  {
    Ticker = ticker;
    Percentual = percentual;
  }

  public void AtualizarPercentual(decimal novoPercentual)
  {
    Percentual = novoPercentual;
  }
}
