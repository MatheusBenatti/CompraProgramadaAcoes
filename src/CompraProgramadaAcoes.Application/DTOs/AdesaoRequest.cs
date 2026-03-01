using System.ComponentModel.DataAnnotations;

namespace CompraProgramadaAcoes.Application.DTOs;

public class AdesaoRequest
{
    [Required(ErrorMessage = "Nome é obrigatório")]
    public string Nome { get; set; } = null!;

    [Required(ErrorMessage = "CPF é obrigatório")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "CPF deve conter 11 dígitos")]
    public string Cpf { get; set; } = null!;

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Valor mensal é obrigatório")]
    [Range(100.00, double.MaxValue, ErrorMessage = "Valor mensal mínimo é R$ 100,00")]
    public decimal ValorMensal { get; set; }
}
