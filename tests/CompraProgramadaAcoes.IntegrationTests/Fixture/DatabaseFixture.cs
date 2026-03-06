using Microsoft.EntityFrameworkCore;
using CompraProgramadaAcoes.Infrastructure.Persistence;

namespace CompraProgramadaAcoes.IntegrationTests.Fixture;

public class DatabaseFixture : IAsyncLifetime
{
    public string ConnectionString { get; private set; } = "Server=localhost;Port=3306;Database=CompraProgramadaAcoes_db;User Id=root;Password=123456;";

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(ConnectionString, ServerVersion.AutoDetect(ConnectionString))
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(ConnectionString, ServerVersion.AutoDetect(ConnectionString))
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.EnsureDeletedAsync();
    }
}
