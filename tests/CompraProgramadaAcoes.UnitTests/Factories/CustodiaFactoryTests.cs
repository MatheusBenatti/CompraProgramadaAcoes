using CompraProgramadaAcoes.Domain.Entities;
using CompraProgramadaAcoes.Domain.Factories;
using FluentAssertions;

namespace CompraProgramadaAcoes.UnitTests.Factories;

public class CustodiaFactoryTests
{
    private readonly CustodiaFactory _factory;

    public CustodiaFactoryTests()
    {
        _factory = new CustodiaFactory();
    }

    [Fact]
    public void Criar_ComContaGraficaIdValido_DeveRetornarCustodia()
    {
        // Arrange
        var contaGraficaId = 123L;

        // Act
        var custodia = _factory.Criar(contaGraficaId);

        // Assert
        custodia.Should().NotBeNull();
        custodia.ContaGraficaId.Should().Be(contaGraficaId);
        custodia.Ticker.Should().BeNull();
        custodia.Quantidade.Should().Be(0);
        custodia.PrecoMedio.Should().Be(0);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999999)]
    public void Criar_ComDiferentesContaGraficaIds_DeveRetornarCustodiasCorretas(long contaGraficaId)
    {
        // Act
        var custodia = _factory.Criar(contaGraficaId);

        // Assert
        custodia.Should().NotBeNull();
        custodia.ContaGraficaId.Should().Be(contaGraficaId);
    }

    [Fact]
    public void Criar_ComContaGraficaIdZero_DeveRetornarCustodia()
    {
        // Arrange
        var contaGraficaId = 0L;

        // Act
        var custodia = _factory.Criar(contaGraficaId);

        // Assert
        custodia.Should().NotBeNull();
        custodia.ContaGraficaId.Should().Be(contaGraficaId);
    }

    [Fact]
    public void Criar_ComContaGraficaIdNegativo_DeveRetornarCustodia()
    {
        // Arrange
        var contaGraficaId = -1L;

        // Act
        var custodia = _factory.Criar(contaGraficaId);

        // Assert
        custodia.Should().NotBeNull();
        custodia.ContaGraficaId.Should().Be(contaGraficaId);
    }

    [Fact]
    public void Criar_MultiplasInstancias_DeveRetornarInstanciasDiferentes()
    {
        // Arrange
        var contaGraficaId = 123L;

        // Act
        var custodia1 = _factory.Criar(contaGraficaId);
        var custodia2 = _factory.Criar(contaGraficaId);

        // Assert
        custodia1.Should().NotBeSameAs(custodia2);
        custodia1.ContaGraficaId.Should().Be(custodia2.ContaGraficaId);
    }

    [Fact]
    public void Criar_VerificarValoresPadrao_DeveEstarZerados()
    {
        // Arrange
        var contaGraficaId = 123L;

        // Act
        var custodia = _factory.Criar(contaGraficaId);

        // Assert
        custodia.Ticker.Should().BeNull();
        custodia.Quantidade.Should().Be(0);
        custodia.PrecoMedio.Should().Be(0);
    }
}
