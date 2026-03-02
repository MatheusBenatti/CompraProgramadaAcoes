namespace CompraProgramadaAcoes.Domain.ValueObjects;

public sealed record Ticker(string Valor)
{
    public static Ticker Create(string ticker)
    {
        if (string.IsNullOrWhiteSpace(ticker))
            throw new ArgumentException("Ticker não pode ser vazio");
            
        var tickerLimpo = ticker.ToUpperInvariant().Trim();
        
        if (!IsValid(tickerLimpo))
            throw new ArgumentException("Ticker inválido. Formato esperado: PETR4, VALE3, etc.");
            
        return new Ticker(tickerLimpo);
    }
    
    public static implicit operator string(Ticker ticker) => ticker.Valor;
    public static implicit operator Ticker(string ticker) => Create(ticker);
    
    private static bool IsValid(string ticker)
    {
        // Regex para validar formato de ticker brasileiro: 4 letras + 1 dígito
        return System.Text.RegularExpressions.Regex.IsMatch(ticker, @"^[A-Z]{4}\d$");
    }
}
