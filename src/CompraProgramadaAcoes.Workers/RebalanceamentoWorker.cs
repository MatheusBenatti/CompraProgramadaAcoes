using CompraProgramadaAcoes.Application.Interfaces;

namespace CompraProgramadaAcoes.Workers;

public class RebalanceamentoWorker(
    ILogger<RebalanceamentoWorker> logger,
    IServiceProvider serviceProvider) : BackgroundService
{
  private readonly ILogger<RebalanceamentoWorker> _logger = logger;
  private readonly IServiceProvider _serviceProvider = serviceProvider;
  private readonly TimeSpan _intervaloExecucao = TimeSpan.FromDays(7); // Verifica semanalmente

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("Worker de Rebalanceamento iniciado - Verificação semanal de desvios");

    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        var agora = DateTime.UtcNow;
        _logger.LogInformation($"Verificando rebalanceamento por desvio de proporção em: {agora:yyyy-MM-dd HH:mm:ss}");

        using (var scope = _serviceProvider.CreateScope())
        {
          var motorRebalanceamento = scope.ServiceProvider.GetRequiredService<IMotorRebalanceamento>();

          // Executar rebalanceamento por desvio de proporção (limite padrão 10%)
          _logger.LogInformation("Iniciando verificação de desvios de proporção");
          await motorRebalanceamento.RebalancearPorDesvioProporcaoAsync(0.10m);
          _logger.LogInformation("Verificação de desvios de proporção concluída");
        }

        await Task.Delay(_intervaloExecucao, stoppingToken);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Erro durante execução do worker de rebalanceamento");
        await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Espera 1 hora em caso de erro
      }
    }

    _logger.LogInformation("Worker de Rebalanceamento finalizado");
  }

  public override async Task StopAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Parando worker de rebalanceamento...");
    await base.StopAsync(cancellationToken);
  }
}
