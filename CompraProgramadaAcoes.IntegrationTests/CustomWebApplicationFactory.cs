using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Moq;
using CompraProgramadaAcoes.Infrastructure.Persistence;
using CompraProgramadaAcoes.Application.Interfaces;
using CompraProgramadaAcoes.Infrastructure.Cache;
using CompraProgramadaAcoes.Infrastructure.Message;
using Testcontainers;
using Testcontainers.MySql;

namespace CompraProgramadaAcoes.IntegrationTests;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>, IAsyncLifetime where TStartup : class
{
    private readonly MySqlContainer _mysqlContainer;
    private string _connectionString = string.Empty;

    public CustomWebApplicationFactory()
    {
        // Configurar container MySQL com Testcontainers
        _mysqlContainer = new MySqlBuilder()
            .WithImage("mysql:8.0")
            .WithDatabase("CompraProgramadaAcoes_Test")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithPortBinding(3307, true) // Porta aleatória automaticamente
            .WithEnvironment("MYSQL_ROOT_PASSWORD", "root123")
            .WithEnvironment("MYSQL_CHARSET", "utf8mb4")
            .WithEnvironment("MYSQL_COLLATION_SERVER", "utf8mb4_unicode_ci")
            .WithCleanUp(true) // Cleanup automático
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Usar configuração específica de teste
            config.AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: true);
            
            // Sobrescrever connection string com o container dinâmico
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"ConnectionStrings:Default", _connectionString}
            };

            config.AddInMemoryCollection(inMemorySettings);
        });

        builder.ConfigureServices(services =>
        {
            // Remover o DbContext existente
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Adicionar DbContext com o container real
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseMySql(_connectionString, 
                    ServerVersion.AutoDetect(_connectionString),
                    b => b.MigrationsAssembly("CompraProgramadaAcoes.Infrastructure"));
            });

            // Mock de serviços externos (Redis e Kafka podem ser mockados para simplificar)
            var cacheServiceMock = new Mock<ICacheService>();
            cacheServiceMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync((string?)null);
            cacheServiceMock.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            services.AddSingleton(cacheServiceMock.Object);

            var messagePublisherMock = new Mock<IMessagePublisher>();
            messagePublisherMock.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            services.AddSingleton(messagePublisherMock.Object);
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        // Iniciar o container antes dos testes
        await _mysqlContainer.StartAsync();
        
        // Obter connection string dinâmica
        _connectionString = _mysqlContainer.GetConnectionString();
        
        Console.WriteLine($"🐬 MySQL Container iniciado: {_mysqlContainer.Id}");
        Console.WriteLine($"🔗 Connection String: {_connectionString}");
    }

    public new async Task DisposeAsync()
    {
        // Parar e remover o container após os testes
        if (_mysqlContainer != null)
        {
            await _mysqlContainer.StopAsync();
            await _mysqlContainer.DisposeAsync();
            Console.WriteLine($"🗑️ MySQL Container removido: {_mysqlContainer.Id}");
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _mysqlContainer?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
        base.Dispose(disposing);
    }
}
