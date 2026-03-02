namespace CompraProgramadaAcoes.Application.Exceptions;

public class ClienteJaInativoException : Exception
{
    public ClienteJaInativoException()
        : base("Cliente já havia saído do produto.")
    {
    }
}
