using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Application.Interfaces;

public interface ICestaCacheService
{
    Task<CestaCacheDTO?> ObterCestaAsync();
    Task SalvarCestaAsync(CestaCacheDTO cesta);
    Task<CestaCacheDTO> GerarCestaDoDiaAsync(IEnumerable<CotacaoB3> cotacoes);
    Task<bool> AtualizarCestaSeNecessarioAsync(IEnumerable<CotacaoB3> cotacoes);
    Task InicializarCestaPadraoAsync();
}
