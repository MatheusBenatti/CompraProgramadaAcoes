using Xunit;

namespace CompraProgramadaAcoes.IntegrationTests;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory<Program> _factory;

    protected IntegrationTestBase()
    {
        _factory = new CustomWebApplicationFactory<Program>();
    }

    public async Task InitializeAsync()
    {
        await _factory.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
    }
}
