using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Confluent.Kafka;
using CompraProgramadaAcoes.Application.Interfaces;
using CompraProgramadaAcoes.Infrastructure.Cache;
using CompraProgramadaAcoes.Infrastructure.Message;
using CompraProgramadaAcoes.Infrastructure.Persistence.Repositories;

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
    services.AddScoped<ICompraProgramadaRepository, CompraProgramadaRepository>();


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


    // KAFKA - CONSUMER
    services.AddHostedService<KafkaConsumerBackgroundService>();


    return services;
  }
}