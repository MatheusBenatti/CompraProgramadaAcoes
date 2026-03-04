using CompraProgramadaAcoes.Application.Interfaces;

namespace CompraProgramadaAcoes.Application.Services;

public class CestaInitializationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CestaInitializationService> _logger;

    public CestaInitializationService(
        IServiceProvider serviceProvider,
        ILogger<CestaInitializationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando serviço de inicialização da cesta Top Five...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            
            // Verificar se já existe cesta no Redis
            var cestaCacheService = scope.ServiceProvider.GetRequiredService<CestaCacheService>();
            var cestaExistente = await cestaCacheService.ObterCestaAsync();
            
            // Tentar gerar cesta a partir do arquivo COTAHIST mais recente
            var cotahistParser = scope.ServiceProvider.GetRequiredService<CotahistParser>();
            var pastaCotacoes = "cotacoes";
            
            if (!Directory.Exists(pastaCotacoes))
            {
                _logger.LogWarning("Pasta de cotações não encontrada: {Pasta}", pastaCotacoes);
                await GerarCestaPadraoAsync(cestaCacheService);
                return;
            }

            // Buscar arquivo COTAHIST mais recente
            var arquivos = Directory.GetFiles(pastaCotacoes, "COTAHIST_D*.TXT")
                .OrderByDescending(f => f)
                .ToList();

            if (!arquivos.Any())
            {
                _logger.LogWarning("Nenhum arquivo COTAHIST encontrado em: {Pasta}", pastaCotacoes);
                await GerarCestaPadraoAsync(cestaCacheService);
                return;
            }

            var arquivoMaisRecente = arquivos.First();
            _logger.LogInformation("Processando arquivo COTAHIST: {Arquivo}", arquivoMaisRecente);

            // Processar arquivo e gerar cesta automaticamente
            await cotahistParser.ParseArquivoAsync(arquivoMaisRecente);
            
            var cestaGerada = await cestaCacheService.ObterCestaAsync();
            if (cestaGerada != null)
            {
                _logger.LogInformation("Cesta Top Five gerada com sucesso:");
                foreach (var item in cestaGerada.Itens)
                {
                    _logger.LogInformation("  {Ticker}: {Percentual}%", item.Ticker, item.Percentual);
                }
            }
            else
            {
                _logger.LogError("Falha ao gerar cesta Top Five");
                await GerarCestaPadraoAsync(cestaCacheService);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante inicialização da cesta Top Five");
            
            // Fallback: gerar cesta padrão
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var cestaCacheService = scope.ServiceProvider.GetRequiredService<CestaCacheService>();
                await GerarCestaPadraoAsync(cestaCacheService);
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "Erro ao gerar cesta padrão como fallback");
            }
        }
    }

    private async Task GerarCestaPadraoAsync(CestaCacheService cestaCacheService)
    {
        _logger.LogInformation("Gerando cesta Top Five padrão como fallback...");
        await cestaCacheService.InicializarCestaPadraoAsync();
        _logger.LogInformation("Cesta padrão gerada com sucesso");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Parando serviço de inicialização da cesta Top Five");
        return Task.CompletedTask;
    }
}
