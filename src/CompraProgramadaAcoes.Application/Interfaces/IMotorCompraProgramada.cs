namespace CompraProgramadaAcoes.Application.Interfaces;

public interface IMotorCompraProgramada
{
    Task ExecutarComprasProgramadasAsync(DateTime dataReferencia);
    Task<bool> DeveExecutarHoje(DateTime data);
}
