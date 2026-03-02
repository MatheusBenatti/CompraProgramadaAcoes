using CompraProgramadaAcoes.Application.Interfaces;
using CompraProgramadaAcoes.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CompraProgramadaAcoes.Workers;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Iniciando Compra Programada Acoes Workers...");

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Adicionar configuração
                services.AddInfrastructure(context.Configuration);
                
                // Adicionar logging
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });

                // Registrar serviços da aplicação
                services.AddScoped<IMotorCompraProgramada, CompraProgramadaAcoes.Application.Services.MotorCompraProgramada>();
                services.AddScoped<IMotorRebalanceamento, CompraProgramadaAcoes.Application.Services.MotorRebalanceamento>();

                // Registrar Workers
                services.AddHostedService<CompraProgramadaAcoes.Application.Workers.CompraProgramadaWorker>();
                services.AddHostedService<RebalanceamentoWorker>();
                
                // Worker de processamento IR (opcional - apenas se não for Development)
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
                if (environment != "Development")
                {
                    services.AddHostedService<IrEventProcessorWorker>();
                }
            })
            .Build();

        Console.WriteLine("Workers configurados. Iniciando execução...");

        await host.RunAsync();
    }
}
