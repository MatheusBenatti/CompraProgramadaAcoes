using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Application.Interfaces;
using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Application.Services;

public class CestaCacheService : ICestaCacheService
{
    private readonly ICacheService _cacheService;
    private const string CESTA_KEY = "cesta:top_five";

    public CestaCacheService(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<CestaCacheDTO?> ObterCestaAsync()
    {
        return await _cacheService.GetAsync<CestaCacheDTO>(CESTA_KEY);
    }

    public async Task SalvarCestaAsync(CestaCacheDTO cesta)
    {
        if (!cesta.IsValida())
            throw new InvalidOperationException("Cesta inválida: deve ter exatamente 5 itens com soma 100%");

        await _cacheService.SetAsync(CESTA_KEY, cesta);
    }

    /// <summary>
    /// Gera e salva cesta Top Five baseada no volume do dia
    /// </summary>
    public async Task<CestaCacheDTO> GerarCestaDoDiaAsync(IEnumerable<Cotacao> cotacoes)
    {
        var analyzer = new TopFiveAnalyzer();
        var novaCesta = analyzer.GerarCestaTopFive(cotacoes);
        
        await SalvarCestaAsync(novaCesta);
        return novaCesta;
    }

    /// <summary>
    /// Atualiza cesta apenas se houver mudança significativa no Top 5
    /// </summary>
    public async Task<bool> AtualizarCestaSeNecessarioAsync(IEnumerable<Cotacao> cotacoes)
    {
        var cestaAtual = await ObterCestaAsync();
        var analyzer = new TopFiveAnalyzer();

        // Se não existe cesta, cria uma nova
        if (cestaAtual == null)
        {
            await GerarCestaDoDiaAsync(cotacoes);
            return true;
        }

        // Verifica se cesta atual ainda é relevante
        if (analyzer.CestaAindaRelevante(cestaAtual, cotacoes))
        {
            return false; // Não precisa atualizar
        }

        // Gera nova cesta com base no volume do dia
        await GerarCestaDoDiaAsync(cotacoes);
        return true;
    }

    /// <summary>
    /// Mantido para compatibilidade, mas não será usado no fluxo principal
    /// </summary>
    [Obsolete("Use GerarCestaDoDiaAsync para cesta baseada no arquivo COTAHIST")]
    public async Task InicializarCestaPadraoAsync()
    {
        var cestaExistente = await ObterCestaAsync();
        if (cestaExistente != null)
            return; // Já existe cesta no Redis

        var cestaPadrao = new CestaCacheDTO
        {
            Nome = "Top Five",
            DataCriacao = DateTime.UtcNow,
            Ativa = true,
            Itens = new List<ItemCestaCacheDTO>
            {
                new() { Ticker = "VALE3", Percentual = 20m },
                new() { Ticker = "PETR4", Percentual = 20m },
                new() { Ticker = "ITUB4", Percentual = 20m },
                new() { Ticker = "BBDC4", Percentual = 20m },
                new() { Ticker = "WEGE3", Percentual = 20m }
            }
        };

        await SalvarCestaAsync(cestaPadrao);
    }
}
