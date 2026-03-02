using CompraProgramadaAcoes.Domain.Entities;
using FluentAssertions;

namespace CompraProgramadaAcoes.UnitTests.Domain.Entities;

public class HistoricoValorMensalTests
{
    [Fact]
    public void Constructor_QuandoDadosValidos_DeveCriarEntidadeCorretamente()
    {
        // Arrange
        var clienteId = 1L;
        var valorAnterior = 3000m;
        var valorNovo = 6000m;

        // Act
        var historico = new HistoricoValorMensal(clienteId, valorAnterior, valorNovo);

        // Assert
        historico.Should().NotBeNull();
        historico.ClienteId.Should().Be(clienteId);
        historico.ValorAnterior.Should().Be(valorAnterior);
        historico.ValorNovo.Should().Be(valorNovo);
        historico.DataAlteracao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_QuandoCriado_DeveTerIdPadrao()
    {
        // Arrange & Act
        var historico = new HistoricoValorMensal(1, 1000, 2000);

        // Assert
        historico.Id.Should().Be(0); // Id padrão para entidades não persistidas
    }

    [Fact]
    public void Constructor_QuandoValoresDiferentes_DeveManterValoresOriginais()
    {
        // Arrange
        var clienteId = 999L;
        var valorAnterior = 1500.75m;
        var valorNovo = 2500.50m;

        // Act
        var historico = new HistoricoValorMensal(clienteId, valorAnterior, valorNovo);

        // Assert
        historico.ClienteId.Should().Be(clienteId);
        historico.ValorAnterior.Should().Be(valorAnterior);
        historico.ValorNovo.Should().Be(valorNovo);
    }

    [Fact]
    public void DataAlteracao_QuandoCriado_DeveSerDataAtual()
    {
        // Arrange & Act
        var historico = new HistoricoValorMensal(1, 1000, 2000);
        var dataEsperada = DateTime.UtcNow;

        // Assert
        historico.DataAlteracao.Should().BeCloseTo(dataEsperada, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Constructor_QuandoValoresZero_DeveAceitar()
    {
        // Arrange & Act
        var historico = new HistoricoValorMensal(1, 0, 0);

        // Assert
        historico.ValorAnterior.Should().Be(0);
        historico.ValorNovo.Should().Be(0);
    }

    [Fact]
    public void Constructor_QuandoValoresDecimais_DeveManterPrecisao()
    {
        // Arrange
        var valorAnterior = 1234.5678m;
        var valorNovo = 9876.5432m;

        // Act
        var historico = new HistoricoValorMensal(1, valorAnterior, valorNovo);

        // Assert
        historico.ValorAnterior.Should().Be(valorAnterior);
        historico.ValorNovo.Should().Be(valorNovo);
    }
}
