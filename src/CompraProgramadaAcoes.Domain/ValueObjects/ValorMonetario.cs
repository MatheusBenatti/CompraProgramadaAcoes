namespace CompraProgramadaAcoes.Domain.ValueObjects;

public sealed record ValorMonetario(decimal Valor)
{
    public static ValorMonetario Create(decimal valor)
    {
        if (valor < 0)
            throw new ArgumentException("Valor monetário não pode ser negativo");
            
        if (valor > 999999999.99m)
            throw new ArgumentException("Valor monetário excede limite máximo");
            
        return new ValorMonetario(Math.Round(valor, 2));
    }
    
    public static implicit operator decimal(ValorMonetario valor) => valor.Valor;
    public static implicit operator ValorMonetario(decimal valor) => Create(valor);
    
    public ValorMonetario Somar(ValorMonetario outro) => Create(Valor + outro.Valor);
    public ValorMonetario Subtrair(ValorMonetario outro) => Create(Valor - outro.Valor);
    public ValorMonetario Multiplicar(decimal fator) => Create(Valor * fator);
    public ValorMonetario Dividir(decimal divisor) => Create(Valor / divisor);
    
    public bool MaiorQue(ValorMonetario outro) => Valor > outro.Valor;
    public bool MenorQue(ValorMonetario outro) => Valor < outro.Valor;
    
    public override string ToString() => Valor.ToString("C2");
}
