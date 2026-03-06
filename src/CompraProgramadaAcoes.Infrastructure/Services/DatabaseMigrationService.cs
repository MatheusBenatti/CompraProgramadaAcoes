using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CompraProgramadaAcoes.Infrastructure.Persistence;

namespace CompraProgramadaAcoes.Infrastructure.Services;

public class DatabaseMigrationService(
    IServiceProvider serviceProvider,
    ILogger<DatabaseMigrationService> logger) : IHostedService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<DatabaseMigrationService> _logger = logger;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando serviço de migração do banco de dados...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Aplicar migrações pendentes
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
            
            if (pendingMigrations.Any())
            {
                _logger.LogInformation("Aplicando {Count} migrações pendentes...", pendingMigrations.Count());
                await context.Database.MigrateAsync(cancellationToken);
                _logger.LogInformation("Migrações aplicadas com sucesso.");
            }
            else
            {
                _logger.LogInformation("Nenhuma migração pendente encontrada. Banco de dados já está atualizado.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante aplicação das migrações do banco de dados");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Serviço de migração do banco de dados finalizado.");
        return Task.CompletedTask;
    }
}
