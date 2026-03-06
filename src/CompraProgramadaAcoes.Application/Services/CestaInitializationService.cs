using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CompraProgramadaAcoes.Application.Interfaces;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Domain.Entities;

namespace CompraProgramadaAcoes.Application.Services;

public class CestaInitializationService(
      IServiceProvider serviceProvider,
      ILogger<CestaInitializationService> logger,
      IConfiguration configuration) : IHostedService
{
  private readonly IServiceProvider _serviceProvider = serviceProvider;
  private readonly ILogger<CestaInitializationService> _logger = logger;
  private readonly IConfiguration _configuration = configuration;

  /// Obtém o caminho completo para a pasta de cotações
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

      // Obter serviços necessários
      var cotahistParser = scope.ServiceProvider.GetRequiredService<CotahistParser>();
      var cestaCacheService = scope.ServiceProvider.GetRequiredService<ICestaCacheService>();
      var pastaCotacoes = ObterCaminhoCotacoes();

      if (!Directory.Exists(pastaCotacoes))
      {
        _logger.LogWarning("Pasta de cotações não encontrada: {Pasta}", pastaCotacoes);
        return;
      }

      // Buscar arquivo COTAHIST mais recente
      var arquivos = Directory.GetFiles(pastaCotacoes, "COTAHIST_D*.TXT")
          .OrderByDescending(f => f)
          .ToList();

      if (arquivos.Count == 0)
      {
        _logger.LogWarning("Nenhum arquivo COTAHIST encontrado em: {Pasta}", pastaCotacoes);
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
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Erro durante inicialização da cesta Top Five");
    }
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
