using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CompraProgramadaAcoes.IntegrationTests.Fixture;

public class ApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.ConfigureAppConfiguration((context, config) =>
    {
      config.AddJsonFile("appsettings.Testing.json", optional: false, reloadOnChange: true);
    });

    builder.ConfigureServices(services =>
    {
    });

    builder.UseEnvironment("Testing");
  }
}
