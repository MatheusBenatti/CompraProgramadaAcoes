using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Confluent.Kafka;
using CompraProgramadaAcoes.Domain.Interfaces;
using CompraProgramadaAcoes.Domain.Services;
using CompraProgramadaAcoes.Infrastructure.Cache;
using CompraProgramadaAcoes.Infrastructure.Message;
using CompraProgramadaAcoes.Infrastructure.Repositories;
using CompraProgramadaAcoes.Infrastructure.Services;
using CompraProgramadaAcoes.Infrastructure.EventHandlers;
using CompraProgramadaAcoes.Domain.Factories;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Application.Interfaces;
using CompraProgramadaAcoes.Infrastructure.Persistence;
using CompraProgramadaAcoes.Application.Services;
using CompraProgramadaAcoes.Application.Interfaces.Services;
using CompraProgramadaAcoes.Application.UseCases.Admin;
using CompraProgramadaAcoes.Application.UseCases.Motor;
using CompraProgramadaAcoes.Application.UseCases.Rebalanceamento;

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
    services.AddScoped<ICotacaoRepository, CotacaoRepository>();
    services.AddScoped<IEventoIRRepository, EventoIRRepository>();
    services.AddScoped<ICotacaoRepository, CotacaoRepository>();
    
    // Domain Services
    services.AddScoped<ICalculadoraDistribuicao, CalculadoraDistribuicao>();
    services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();
    services.AddScoped<ICotacaoService, CotacaoService>();

    // Factories
    services.AddScoped<IClienteFactory, ClienteFactory>();
    services.AddScoped<IContaGraficaFactory, ContaGraficaFactory>();
    services.AddScoped<ICustodiaFactory, CustodiaFactory>();

    // Use Cases
    services.AddScoped<CompraProgramadaAcoes.Application.Interfaces.UseCases.IRealizarAdesao, CompraProgramadaAcoes.Application.UseCases.RealizarAdesao>();
    services.AddScoped<CompraProgramadaAcoes.Application.Interfaces.UseCases.IRealizarSaida, CompraProgramadaAcoes.Application.UseCases.RealizarSaida>();
    services.AddScoped<CompraProgramadaAcoes.Application.Interfaces.UseCases.IAlterarValorMensal, CompraProgramadaAcoes.Application.UseCases.AlterarValorMensal>();
    services.AddScoped<CompraProgramadaAcoes.Application.Interfaces.UseCases.IConsultarCarteira, CompraProgramadaAcoes.Application.Services.ConsultarCarteira>();
    services.AddScoped<CompraProgramadaAcoes.Application.Interfaces.UseCases.IConsultarRentabilidade, CompraProgramadaAcoes.Application.Services.ConsultarRentabilidade>();

    // Admin Services and Use Cases
    services.AddScoped<IAdminService, AdminService>();
    services.AddScoped<CadastrarCestaUseCase>();
    services.AddScoped<ObterCestaAtualUseCase>();
    services.AddScoped<ObterHistoricoCestasUseCase>();
    services.AddScoped<ObterCustodiaMasterUseCase>();

    // Motor Services and Use Cases
    services.AddScoped<IMotorService, MotorService>();
    services.AddScoped<ExecutarCompraUseCase>();
    
    // Motor Interfaces
    services.AddScoped<IMotorCompraProgramada, MotorCompraProgramada>();
    services.AddScoped<IMotorRebalanceamento, MotorRebalanceamento>();

    // Rebalanceamento Services and Use Cases
    services.AddScoped<IRebalanceamentoService, RebalanceamentoService>();
    services.AddScoped<RebalancearPorMudancaCestaUseCase>();

    // REDIS
    services.Configure<RedisSettings>(
        configuration.GetSection("Redis"));

    services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
      var settings = sp.GetRequiredService<IOptions<RedisSettings>>().Value;
      var config = ConfigurationOptions.Parse(settings.Connection);
      config.AbortOnConnectFail = false;
      config.ConnectRetry = 3;
      config.ConnectTimeout = 5000;
      return ConnectionMultiplexer.Connect(config);
    });

    services.AddScoped<ICacheService, RedisCacheService>();
    services.AddScoped<ICestaCacheService, CestaCacheService>();
    services.AddScoped<ICotacaoCacheService, CotacaoCacheService>();
    services.AddScoped<CotacaoCacheService>();
    services.AddScoped<CotahistParser>();
    services.AddScoped<TopFiveAnalyzer>();
    
    // Serviços hospedados - DatabaseMigrationService deve ser registrado primeiro para rodar antes do parser
    services.AddHostedService<DatabaseMigrationService>();
    services.AddHostedService<CestaInitializationService>();

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

    // EVENT HANDLERS
    services.AddScoped<ClienteEventHandler>();
    services.AddScoped<ContaEventHandler>();
    services.AddScoped<OrdemCompraEventHandler>();

    // KAFKA - CONSUMER 
    if (configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Production")
    {
      services.AddHostedService<KafkaConsumerBackgroundService>();
    }
      
    return services;
  }
}
