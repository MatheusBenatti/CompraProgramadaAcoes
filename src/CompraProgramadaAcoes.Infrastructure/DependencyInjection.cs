using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Confluent.Kafka;
using CompraProgramadaAcoes.Application.Interfaces;
using CompraProgramadaAcoes.Infrastructure.Cache;
using CompraProgramadaAcoes.Infrastructure.Message;
using CompraProgramadaAcoes.Infrastructure.Persistence;
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
        var redisConnection = configuration["Redis:Connection"] ?? "localhost:6379";

        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisConnection));

        services.AddScoped<ICacheService, RedisCacheService>();


        // KAFKA - PRODUCER
        var kafkaConfig = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"]
        };

        services.AddSingleton<IProducer<string, string>>(sp =>
            new ProducerBuilder<string, string>(kafkaConfig).Build());

        services.AddScoped<IMessagePublisher, KafkaPublisher>();


        // KAFKA - CONSUMER
        services.AddHostedService<KafkaConsumerBackgroundService>();


        return services;
    }
}