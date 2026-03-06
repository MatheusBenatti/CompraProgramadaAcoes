using CompraProgramadaAcoes.Application.Services;
using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Domain.Entities;
using FluentAssertions;

namespace CompraProgramadaAcoes.UnitTests.Application;

public class TopFiveAnalyzerTests
{
  [Fact]
  public void GerarCestaTopFive_ComCincoCotacoesValidas_RetornaCestaValida()
  {
    // Arrange
    var cotacoes = new List<Cotacao>
        {
            new() { Ticker = "aaa1", TipoMercado = 10, VolumeNegociado = 100 },
            new() { Ticker = "bbb2", TipoMercado = 10, VolumeNegociado = 200 },
            new() { Ticker = "ccc3", TipoMercado = 10, VolumeNegociado = 300 },
            new() { Ticker = "ddd4", TipoMercado = 10, VolumeNegociado = 400 },
            new() { Ticker = "eee5", TipoMercado = 10, VolumeNegociado = 0 }, // should be filtered out
            new() { Ticker = "fff6", TipoMercado = 10, VolumeNegociado = 500 }
        };

    // Use only cotacoes with Volume > 0; ensure we have at least 5 after filtering
    var analyzer = new TopFiveAnalyzer();

    // Act
    var cesta = analyzer.GerarCestaTopFive(cotacoes);

    // Assert
    cesta.Should().NotBeNull();
    cesta.Itens.Should().HaveCount(5);
    cesta.Nome.Should().Be("Top Five");
    cesta.Itens.Select(i => i.Ticker).Should().OnlyContain(t => t == t.ToUpper());
    // Percentuais somam aproximadamente 100
    Math.Abs(cesta.Itens.Sum(i => i.Percentual) - 100).Should().BeLessThan(0.02m);
    cesta.IsValida().Should().BeTrue();
  }

  [Fact]
  public void GerarCestaTopFive_MenosDeCincoCotacoes_LancaInvalidOperationException()
  {
    // Arrange
    var cotacoes = new List<Cotacao>
        {
            new Cotacao { Ticker = "a", TipoMercado = 10, VolumeNegociado = 10 },
            new Cotacao { Ticker = "b", TipoMercado = 10, VolumeNegociado = 20 },
            new Cotacao { Ticker = "c", TipoMercado = 10, VolumeNegociado = 30 }
        };

    var analyzer = new TopFiveAnalyzer();

    // Act & Assert
    Action act = () => analyzer.GerarCestaTopFive(cotacoes);
    act.Should().Throw<InvalidOperationException>().WithMessage("Apenas * ações encontradas com volume negociado. Mínimo necessário: 5");
  }

  [Fact]
  public void ObterEstatisticasTopFive_RetornaTop5OrdenadoPorVolume()
  {
    // Arrange
    var cotacoes = new List<Cotacao>
        {
            new() { Ticker = "A", TipoMercado = 10, VolumeNegociado = 100 },
            new() { Ticker = "B", TipoMercado = 10, VolumeNegociado = 500 },
            new() { Ticker = "C", TipoMercado = 10, VolumeNegociado = 300 },
            new() { Ticker = "D", TipoMercado = 10, VolumeNegociado = 200 },
            new() { Ticker = "E", TipoMercado = 10, VolumeNegociado = 150 },
            new() { Ticker = "F", TipoMercado = 10, VolumeNegociado = 50 }
        };

    // Act
    var stats = TopFiveAnalyzer.ObterEstatisticasTopFive(cotacoes);

    // Assert
    stats.Should().HaveCount(5);
    // The highest should be B
    stats.Keys.First().Should().Be("B");
    stats["B"].Should().Be(500);
  }

  [Fact]
  public void CestaAindaRelevante_RetornaTrueQuandoTresTickersEmComum()
  {
    // Arrange
    var cesta = new CestaCacheDTO
    {
      Itens =
            [
                new ItemCestaCacheDTO { Ticker = "A", Percentual = 20 },
                new ItemCestaCacheDTO { Ticker = "B", Percentual = 20 },
                new ItemCestaCacheDTO { Ticker = "C", Percentual = 20 },
                new ItemCestaCacheDTO { Ticker = "D", Percentual = 20 },
                new ItemCestaCacheDTO { Ticker = "E", Percentual = 20 }
            ]
    };

    var novas = new List<Cotacao>
        {
            new() { Ticker = "A", TipoMercado = 10, VolumeNegociado = 100 },
            new() { Ticker = "B", TipoMercado = 10, VolumeNegociado = 90 },
            new() { Ticker = "C", TipoMercado = 10, VolumeNegociado = 80 },
            new() { Ticker = "X", TipoMercado = 10, VolumeNegociado = 70 },
            new() { Ticker = "Y", TipoMercado = 10, VolumeNegociado = 60 }
        };

    // Act
    var relevante = TopFiveAnalyzer.CestaAindaRelevante(cesta, novas);

    // Assert
    relevante.Should().BeTrue();
  }

  [Fact]
  public void CestaAindaRelevante_RetornaFalseQuandoMenosDeTresEmComum()
  {
    // Arrange
    var cesta = new CestaCacheDTO
    {
      Itens =
            [
                new ItemCestaCacheDTO { Ticker = "A", Percentual = 20 },
                new ItemCestaCacheDTO { Ticker = "B", Percentual = 20 },
                new ItemCestaCacheDTO { Ticker = "C", Percentual = 20 },
                new ItemCestaCacheDTO { Ticker = "D", Percentual = 20 },
                new ItemCestaCacheDTO { Ticker = "E", Percentual = 20 }
            ]
    };

    var novas = new List<Cotacao>
        {
            new() { Ticker = "A", TipoMercado = 10, VolumeNegociado = 100 },
            new() { Ticker = "X", TipoMercado = 10, VolumeNegociado = 90 },
            new() { Ticker = "Y", TipoMercado = 10, VolumeNegociado = 80 },
            new() { Ticker = "Z", TipoMercado = 10, VolumeNegociado = 70 },
            new() { Ticker = "W", TipoMercado = 10, VolumeNegociado = 60 }
        };

    // Act
    var relevante = TopFiveAnalyzer.CestaAindaRelevante(cesta, novas);

    // Assert
    relevante.Should().BeFalse();
  }

  [Fact]
  public void CestaCacheDTO_IsValida_VerificaCondicoes()
  {
    var cesta = new CestaCacheDTO
    {
      Ativa = true,
      Itens =
            [
                new ItemCestaCacheDTO { Ticker = "A", Percentual = 20 },
                new ItemCestaCacheDTO { Ticker = "B", Percentual = 20 },
                new ItemCestaCacheDTO { Ticker = "C", Percentual = 20 },
                new ItemCestaCacheDTO { Ticker = "D", Percentual = 20 },
                new ItemCestaCacheDTO { Ticker = "E", Percentual = 19.9m }
            ]
    };

    cesta.IsValida().Should().BeFalse();

    // Ajustar para 100
    cesta.Itens[4].Percentual = 20;
    cesta.IsValida().Should().BeTrue();
  }
}
