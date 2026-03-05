using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CompraProgramadaAcoes.Application.Interfaces;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Application.Services;

public class CestaInitializationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CestaInitializationService> _logger;
    private readonly IConfiguration _configuration;

    public CestaInitializationService(
        IServiceProvider serviceProvider,
        ILogger<CestaInitializationService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Obtém o caminho completo para a pasta de cotações
    /// </summary>
    private string ObterCaminhoCotacoes()
    {
        var path = _configuration["FileStorage:CotacoesPath"] ?? "cotacoes";
        
        // Se estiver rodando no Docker, usa caminho absoluto /app/cotacoes
        var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
        if (isDocker)
        {
            return "/app/cotacoes";
        }
        
        // Para desenvolvimento local, usa Path.GetFullPath
        return Path.GetFullPath(path);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando serviço de inicialização da cesta Top Five...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            
            // Verificar se já existe cesta no Redis
            var cestaCacheService = scope.ServiceProvider.GetRequiredService<ICestaCacheService>();
            var cestaExistente = await cestaCacheService.ObterCestaAsync();
            
            if (cestaExistente != null)
            {
                _logger.LogInformation("Cesta Top Five já existe no Redis. Pulando inicialização.");
                return;
            }

            // Tentar gerar cesta a partir do arquivo COTAHIST mais recente
            var cotahistParser = scope.ServiceProvider.GetRequiredService<CotahistParser>();
            var pastaCotacoes = ObterCaminhoCotacoes();
            
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
                
                // Salvar a cesta também no banco de dados
                await SalvarCestaNoBancoAsync(cestaGerada);
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
                var cestaCacheService = scope.ServiceProvider.GetRequiredService<ICestaCacheService>();
                await GerarCestaPadraoAsync(cestaCacheService);
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "Erro ao gerar cesta padrão como fallback");
            }
        }
    }

    private async Task GerarCestaPadraoAsync(ICestaCacheService cestaCacheService)
    {
        _logger.LogInformation("Gerando cesta Top Five padrão como fallback...");
        await cestaCacheService.InicializarCestaPadraoAsync();
        
        // Tentar salvar a cesta padrão no banco também
        var cestaPadrao = await cestaCacheService.ObterCestaAsync();
        if (cestaPadrao != null)
        {
            await SalvarCestaNoBancoAsync(cestaPadrao);
        }
        
        _logger.LogInformation("Cesta padrão gerada com sucesso");
    }

    private async Task SalvarCestaNoBancoAsync(CestaCacheDTO cestaCacheDTO)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var cestaRepository = scope.ServiceProvider.GetRequiredService<ICestaRecomendacaoRepository>();
            
            // Verificar se já existe uma cesta ativa
            var cestaExistente = await cestaRepository.ObterCestaVigenteAsync();
            
            if (cestaExistente != null)
            {
                // Desativar cesta existente
                cestaExistente.Desativar();
                await cestaRepository.SaveChangesAsync();
            }
            
            // Criar nova cesta
            var novaCesta = new CestaRecomendacao();
            novaCesta.AtualizarNome(cestaCacheDTO.Nome ?? "Top Five");
            
            // Adicionar itens usando o método AdicionarItem
            foreach (var item in cestaCacheDTO.Itens)
            {
                novaCesta.AdicionarItem(item.Ticker, item.Percentual);
            }
            
            await cestaRepository.AddAsync(novaCesta);
            await cestaRepository.SaveChangesAsync();
            
            _logger.LogInformation("Cesta salva com sucesso no banco de dados: {CestaId}", novaCesta.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar cesta no banco de dados");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Parando serviço de inicialização da cesta Top Five");
        return Task.CompletedTask;
    }
}
