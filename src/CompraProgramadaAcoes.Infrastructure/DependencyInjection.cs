using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Confluent.Kafka;
using CompraProgramadaAcoes.Application.UseCases;
using CompraProgramadaAcoes.Domain.Interfaces;
using CompraProgramadaAcoes.Domain.Services;
using CompraProgramadaAcoes.Infrastructure.Cache;
using CompraProgramadaAcoes.Infrastructure.Message;
using CompraProgramadaAcoes.Infrastructure.Repositories;
using CompraProgramadaAcoes.Infrastructure.Services;
using CompraProgramadaAcoes.Domain.Factories;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Infrastructure.Persistence;

namespace CompraProgramadaAcoes.Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(
      this IServiceCollection services,
      IConfiguration configuration)
  {
    var connectionString = configuration.GetConnectionString("Default");

    // DATABASE (MySQL)
    services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString),
            b => b.MigrationsAssembly("CompraProgramadaAcoes.Infrastructure")
        ));

    // Repositories
    services.AddScoped<IClienteRepository, ClienteRepository>();
    services.AddScoped<IContaGraficaRepository, ContaGraficaRepository>();
    services.AddScoped<ICustodiaRepository, CustodiaRepository>();
    services.AddScoped<IHistoricoValorMensalRepository, HistoricoValorMensalRepository>();
    services.AddScoped<ICestaRecomendacaoRepository, CestaRecomendacaoRepository>();
    services.AddScoped<IOrdemCompraRepository, OrdemCompraRepository>();
    services.AddScoped<IDistribuicaoRepository, DistribuicaoRepository>();
    services.AddScoped<IContaMasterRepository, ContaMasterRepository>();
    services.AddScoped<IEventoIRRepository, EventoIRRepository>();

    // Domain Services
    services.AddScoped<ICalculadoraDistribuicao, CalculadoraDistribuicao>();
    services.AddScoped<IEventPublisher, DomainEventPublisher>();
    services.AddScoped<ICotacaoService, CotacaoService>();

    // Factories
    services.AddScoped<IClienteFactory, ClienteFactory>();
    services.AddScoped<IContaGraficaFactory, ContaGraficaFactory>();
    services.AddScoped<ICustodiaFactory, CustodiaFactory>();

    // Use Cases
    services.AddScoped<RealizarAdesao>();
    services.AddScoped<RealizarSaida>();
    services.AddScoped<AlterarValorMensal>();

    // REDIS
    services.Configure<RedisSettings>(
        configuration.GetSection("Redis"));

    services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        var settings = sp.GetRequiredService<IOptions<RedisSettings>>().Value;
        return ConnectionMultiplexer.Connect(settings.Connection);
    });

    services.AddScoped<ICacheService, RedisCacheService>();

    // KAFKA - CONFIGURAÇÃO
    services.Configure<KafkaSettings>(
        configuration.GetSection("Kafka"));

    // KAFKA - PRODUCER
    services.AddSingleton<IProducer<string, string>>(sp =>
    {
        var settings = sp.GetRequiredService<IOptions<KafkaSettings>>().Value;
        var kafkaConfig = new ProducerConfig
        {
            BootstrapServers = settings.BootstrapServers
        };
        return new ProducerBuilder<string, string>(kafkaConfig).Build();
    });

    services.AddScoped<IMessagePublisher, KafkaPublisher>();

    // KAFKA - CONSUMER (apenas em ambiente de produção/Docker)
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
    if (environment != "Development")
    {
      services.AddHostedService<KafkaConsumerBackgroundService>();
    }

    return services;
  }
}