namespace CompraProgramadaAcoes.Application.Exceptions;

public class ValorMensalInvalidoException : Exception
{
    public ValorMensalInvalidoException()
        : base("Valor mensal abaixo do mínimo.")
    {
    }
}
