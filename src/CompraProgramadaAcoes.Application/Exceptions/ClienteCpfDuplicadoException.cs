namespace CompraProgramadaAcoes.Application.Exceptions
{
  public class ClienteCpfDuplicadoException : Exception
  {
    public ClienteCpfDuplicadoException()
        : base("CPF ja cadastrado no sistema.")
    {
    }
  }
}
