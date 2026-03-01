namespace CompraProgramadaAcoes.Application.DTOs;

public class AdesaoResponse
{
    public long ClienteId { get; set; }
    public string Nome { get; set; } = null!;
    public string Cpf { get; set; } = null!;
    public string Email { get; set; } = null!;
    public decimal ValorMensal { get; set; }
    public bool Ativo { get; set; }
    public string DataAdesao { get; set; } = default!;
    public ContaGraficaResponse ContaGrafica { get; set; } = null!;
}

public class ContaGraficaResponse
{
    public long Id { get; set; }
    public string NumeroConta { get; set; } = null!;
    public string Tipo { get; set; } = null!;
    public string DataCriacao { get; set; } = default!;
}
