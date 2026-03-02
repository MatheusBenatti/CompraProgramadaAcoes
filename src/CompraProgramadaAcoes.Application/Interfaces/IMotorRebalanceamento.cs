using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Application.Interfaces;

public interface IMotorRebalanceamento
{
    Task RebalancearPorMudancaCestaAsync(CestaRecomendacao cestaAntiga, CestaRecomendacao cestaNova);
    Task RebalancearPorDesvioProporcaoAsync(decimal limiteDesvioPercentual = 0.10m); // 10% por padrão
}
