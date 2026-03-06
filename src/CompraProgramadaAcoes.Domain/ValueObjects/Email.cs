namespace CompraProgramadaAcoes.Domain.ValueObjects;

public sealed record Email(string Valor)
{
  public static Email Create(string email)
  {
    if (string.IsNullOrWhiteSpace(email))
      throw new ArgumentException("Email não pode ser vazio");

    if (!IsValid(email))
      throw new ArgumentException("Email inválido");

    return new Email(email.ToLowerInvariant().Trim());
  }

  public static implicit operator string(Email email) => email.Valor;
  public static implicit operator Email(string email) => Create(email);

  private static bool IsValid(string email)
  {
    try
    {
      var addr = new System.Net.Mail.MailAddress(email);
      return addr.Address == email;
    }
    catch
    {
      return false;
    }
  }
}
