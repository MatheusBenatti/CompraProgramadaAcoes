using System.ComponentModel.DataAnnotations;

namespace CompraProgramadaAcoes.Application.DTOs;

public class AlterarValorMensalRequest
{
    [Required(ErrorMessage = "Novo valor mensal é obrigatório")]
    [Range(100.00, double.MaxValue, ErrorMessage = "Valor mensal mínimo é R$ 100,00")]
    public decimal NovoValorMensal { get; set; }
}
