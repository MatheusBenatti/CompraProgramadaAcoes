using CompraProgramadaAcoes.Application.Services;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Application.Interfaces;
using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using FluentAssertions;

namespace CompraProgramadaAcoes.UnitTests.Application
{
  public class MotorCompraProgramadaTests
  {
    private readonly Mock<IClienteRepository> _clienteRepository = new();
    private readonly Mock<IContaMasterRepository> _contaMasterRepository = new();
    private readonly Mock<ICustodiaRepository> _custodiaRepository = new();
    private readonly Mock<ICestaCacheService> _cestaCacheService = new();
    private readonly Mock<ICacheService> _cacheService = new();
    private readonly Mock<ICotacaoCacheService> _cotacaoCacheService = new();
    private readonly Mock<IOrdemCompraRepository> _ordemCompraRepository = new();
    private readonly Mock<IDistribuicaoRepository> _distribuicaoRepository = new();
    private readonly Mock<IMessagePublisher> _messagePublisher = new();
    private readonly Mock<CotahistParser> _cotahistParser;
    private readonly Mock<ILogger<MotorCompraProgramada>> _logger = new();
    private readonly Mock<IConfiguration> _configuration = new();

    public MotorCompraProgramadaTests()
    {
      // _cotacaoCacheService já inicializado como Mock<ICotacaoCacheService>
      _cotahistParser = new Mock<CotahistParser>(Mock.Of<ICestaCacheService>(), _cotacaoCacheService.Object, Mock.Of<ICotacaoRepository>());

      // Default configuration values
      _configuration.Setup(c => c[It.IsAny<string>()]).Returns((string?)null);
    }

    [Fact]
    public async Task DeveExecutarHoje_RetornaFalseParaFimDeSemana()
    {
      var sut = CreateSut();

      var sabado = await sut.DeveExecutarHoje(new DateTime(2026, 3, 7)); // Saturday
      var domingo = await sut.DeveExecutarHoje(new DateTime(2026, 3, 8)); // Sunday

      sabado.Should().BeFalse();
      domingo.Should().BeFalse();
    }

    [Fact]
    public async Task DeveExecutarHoje_RetornaTrueParaDiasPermitidos()
    {
      var sut = CreateSut();

      var dia5 = await sut.DeveExecutarHoje(new DateTime(2026, 3, 5));
      var dia15 = await sut.DeveExecutarHoje(new DateTime(2026, 3, 15));
      var dia25 = await sut.DeveExecutarHoje(new DateTime(2026, 3, 25));

      dia5.Should().BeTrue();
      dia15.Should().BeTrue();
      dia25.Should().BeTrue();
    }

    [Fact]
    public async Task ExecutarComprasProgramadasAsync_SemClientes_NaoExecutaCesta()
    {
      // Arrange
      var sut = CreateSut();
      _clienteRepository.Setup(x => x.ObterClientesAtivosAsync()).ReturnsAsync(new List<Cliente>());

      // Act
      await sut.ExecutarComprasProgramadasAsync(new DateTime(2026, 3, 5));

      // Assert: quando n�o h� clientes, n�o pede cesta
      _cestaCacheService.Verify(x => x.ObterCestaAsync(), Times.Never);
    }

    [Fact]
    public async Task ExecutarComprasProgramadasAsync_SemCesta_NaoProssegue()
    {
      var sut = CreateSut();

      var cliente = new Cliente("Nome", "12345678901", "email@teste.com", 300m);
      var conta = new ContaGrafica(null);
      cliente.AssociarContaGrafica(conta);

      _clienteRepository.Setup(x => x.ObterClientesAtivosAsync()).ReturnsAsync(new List<Cliente> { cliente });
      _cestaCacheService.Setup(x => x.ObterCestaAsync()).ReturnsAsync((CestaCacheDTO?)null);

      await sut.ExecutarComprasProgramadasAsync(new DateTime(2026, 3, 5));

      // N�o deve chamar cota��o quando n�o h� cesta
      _cotacaoCacheService.Verify(x => x.ObterPrecosFechamentoAsync(It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    private MotorCompraProgramada CreateSut()
    {
      return new MotorCompraProgramada(
          _clienteRepository.Object,
          _contaMasterRepository.Object,
          _custodiaRepository.Object,
          _cestaCacheService.Object,
          _cotacaoCacheService.Object,
          _ordemCompraRepository.Object,
          _distribuicaoRepository.Object,
          _messagePublisher.Object,
          _cotahistParser.Object,
          _logger.Object,
          _configuration.Object);
    }
  }
}
