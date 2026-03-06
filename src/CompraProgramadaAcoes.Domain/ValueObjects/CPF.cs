namespace CompraProgramadaAcoes.Domain.ValueObjects;

public sealed record CPF(string Valor)
{
  public static CPF Create(string cpf)
  {
    if (string.IsNullOrWhiteSpace(cpf))
      throw new ArgumentException("CPF não pode ser vazio");

    var cpfLimpo = new string([.. cpf.Where(char.IsDigit)]);

    if (cpfLimpo.Length != 11)
      throw new ArgumentException("CPF deve conter 11 dígitos");

    if (!IsValid(cpfLimpo))
      throw new ArgumentException("CPF inválido");

    return new CPF(cpfLimpo);
  }

  public static implicit operator string(CPF cpf) => cpf.Valor;
  public static implicit operator CPF(string cpf) => Create(cpf);

  private static bool IsValid(string cpf)
  {
    // Algoritmo de validação de CPF
    if (cpf.Length != 11) return false;

    // Verificar se todos os dígitos são iguais
    if (cpf.Distinct().Count() == 1) return false;

    // Calcular dígitos verificadores
    var multiplicadores1 = new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
    var multiplicadores2 = new[] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

    var soma1 = cpf.Take(9).Select((c, i) => int.Parse(c.ToString()) * multiplicadores1[i]).Sum();
    var resto1 = soma1 % 11;
    var digito1 = resto1 < 2 ? 0 : 11 - resto1;

    var soma2 = cpf.Take(10).Select((c, i) => int.Parse(c.ToString()) * multiplicadores2[i]).Sum();
    var resto2 = soma2 % 11;
    var digito2 = resto2 < 2 ? 0 : 11 - resto2;

    return cpf[9] == digito1.ToString()[0] && cpf[10] == digito2.ToString()[0];
  }
}
