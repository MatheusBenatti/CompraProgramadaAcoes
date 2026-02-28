using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
    var redisConnection = configuration["Redis:Connection"]
        ?? throw new InvalidOperationException("Redis connection not configured.");

    services.AddSingleton<IConnectionMultiplexer>(sp =>
        ConnectionMultiplexer.Connect(redisConnection));

    services.AddScoped<ICacheService, RedisCacheService>();


    // KAFKA - PRODUCER
    var bootstrapServers = configuration["Kafka:BootstrapServers"]
    ?? throw new InvalidOperationException("Kafka BootstrapServers not configured.");

    var kafkaConfig = new ProducerConfig
    {
      BootstrapServers = bootstrapServers
    };

    services.AddSingleton<IProducer<string, string>>(sp =>
        new ProducerBuilder<string, string>(kafkaConfig).Build());

    services.AddScoped<IMessagePublisher, KafkaPublisher>();


    // KAFKA - CONSUMER
    services.AddHostedService<KafkaConsumerBackgroundService>();


    return services;
  }
}