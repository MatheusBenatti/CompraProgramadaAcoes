using System.ComponentModel.DataAnnotations.Schema;

namespace CompraProgramadaAcoes.Domain.Entities;

public class Cotacao
{
    public long Id { get; set; }
    public DateTime DataPregao { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public decimal PrecoAbertura { get; set; }
    public decimal PrecoFechamento { get; set; }
    public decimal PrecoMaximo { get; set; }
    public decimal PrecoMinimo { get; set; }
    
    [NotMapped]
    public string CodigoBDI { get; set; } = string.Empty;
    
    [NotMapped]
    public int TipoMercado { get; set; }
    
    [NotMapped]
    public string NomeEmpresa { get; set; } = string.Empty;
    
    [NotMapped]
    public decimal PrecoMedio { get; set; }
    
    [NotMapped]
    public long QuantidadeNegociada { get; set; }
    
    [NotMapped]
    public decimal VolumeNegociado { get; set; }
}
