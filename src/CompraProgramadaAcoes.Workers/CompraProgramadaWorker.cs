using CompraProgramadaAcoes.Application.Interfaces;

namespace CompraProgramadaAcoes.Workers;

public class CompraProgramadaWorker(
    ILogger<CompraProgramadaWorker> logger,
    IServiceProvider serviceProvider) : BackgroundService
{
  private readonly ILogger<CompraProgramadaWorker> _logger = logger;
  private readonly IServiceProvider _serviceProvider = serviceProvider;
  private readonly TimeSpan _intervaloExecucao = TimeSpan.FromHours(1); // Verifica a cada hora

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("Worker de Compra Programada iniciado");

    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        var agora = DateTime.UtcNow;
        _logger.LogInformation($"Verificando execuções de compra programada em: {agora:yyyy-MM-dd HH:mm:ss}");

        using (var scope = _serviceProvider.CreateScope())
        {
          var motorCompra = scope.ServiceProvider.GetRequiredService<IMotorCompraProgramada>();

          if (await motorCompra.DeveExecutarHoje(agora))
          {
            _logger.LogInformation("Iniciando execução de compras programadas");
            await motorCompra.ExecutarComprasProgramadasAsync(agora);
            _logger.LogInformation("Execução de compras programadas concluída com sucesso");
          }
          else
          {
            _logger.LogDebug("Hoje não é dia de execução de compras programadas");
          }
        }

        await Task.Delay(_intervaloExecucao, stoppingToken);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Erro durante execução do worker de compra programada");
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Espera 5 minutos em caso de erro
      }
    }

    _logger.LogInformation("Worker de Compra Programada finalizado");
  }

  public override async Task StopAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Parando worker de compra programada...");
    await base.StopAsync(cancellationToken);
  }
}
