using CompraProgramadaAcoes.Domain.Entities;
using CompraProgramadaAcoes.Domain.Factories;
using FluentAssertions;

namespace CompraProgramadaAcoes.UnitTests.Factories;

public class ContaGraficaFactoryTests
{
    private readonly ContaGraficaFactory _factory;

    public ContaGraficaFactoryTests()
    {
        _factory = new ContaGraficaFactory();
    }

    [Fact]
    public void Criar_ComClienteIdValido_DeveRetornarContaGrafica()
    {
        // Arrange
        var clienteId = 123L;

        // Act
        var contaGrafica = _factory.Criar(clienteId);

        // Assert
        contaGrafica.Should().NotBeNull();
        contaGrafica.ClienteId.Should().Be(clienteId);
        contaGrafica.Tipo.Should().Be("FILHOTE");
        contaGrafica.NumeroConta.Should().BeNull();
        contaGrafica.DataCriacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        contaGrafica.Custodias.Should().BeEmpty();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999999)]
    public void Criar_ComDiferentesClienteIds_DeveRetornarContasCorretas(long clienteId)
    {
        // Act
        var contaGrafica = _factory.Criar(clienteId);

        // Assert
        contaGrafica.Should().NotBeNull();
        contaGrafica.ClienteId.Should().Be(clienteId);
    }

    [Fact]
    public void Criar_ComClienteIdZero_DeveRetornarContaGrafica()
    {
        // Arrange
        var clienteId = 0L;

        // Act
        var contaGrafica = _factory.Criar(clienteId);

        // Assert
        contaGrafica.Should().NotBeNull();
        contaGrafica.ClienteId.Should().Be(clienteId);
    }

    [Fact]
    public void Criar_ComClienteIdNegativo_DeveRetornarContaGrafica()
    {
        // Arrange
        var clienteId = -1L;

        // Act
        var contaGrafica = _factory.Criar(clienteId);

        // Assert
        contaGrafica.Should().NotBeNull();
        contaGrafica.ClienteId.Should().Be(clienteId);
    }

    [Fact]
    public void Criar_MultiplasInstancias_DeveRetornarInstanciasDiferentes()
    {
        // Arrange
        var clienteId = 123L;

        // Act
        var conta1 = _factory.Criar(clienteId);
        var conta2 = _factory.Criar(clienteId);

        // Assert
        conta1.Should().NotBeSameAs(conta2);
        conta1.ClienteId.Should().Be(conta2.ClienteId);
    }

    [Fact]
    public void Criar_VerificarTipoPadrao_DeveSerFilhote()
    {
        // Arrange
        var clienteId = 123L;

        // Act
        var contaGrafica = _factory.Criar(clienteId);

        // Assert
        contaGrafica.Tipo.Should().Be("FILHOTE");
    }

    [Fact]
    public void Criar_VerificarDataCriacao_DeveSerAtual()
    {
        // Arrange
        var clienteId = 123L;
        var antes = DateTime.UtcNow;

        // Act
        var contaGrafica = _factory.Criar(clienteId);
        var depois = DateTime.UtcNow;

        // Assert
        contaGrafica.DataCriacao.Should().BeOnOrAfter(antes);
        contaGrafica.DataCriacao.Should().BeOnOrBefore(depois);
    }
}
