using CompraProgramadaAcoes.Infrastructure.Services;
using CompraProgramadaAcoes.Domain.Services;
using CompraProgramadaAcoes.Domain.ValueObjects;
using CompraProgramadaAcoes.Domain.Entities;
using CompraProgramadaAcoes.Application.Services;
using FluentAssertions;
using Moq;

namespace CompraProgramadaAcoes.UnitTests.Services;

public class CotacaoServiceTests
{
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<CestaCacheService> _cestaCacheServiceMock;
    private readonly Mock<CotacaoCacheService> _cotacaoCacheServiceMock;
    private readonly Mock<CotahistParser> _cotahistParserMock;
    private readonly CotacaoService _cotacaoService;
    private readonly string _pastaCotacoes = "test_cotacoes";

    public CotacaoServiceTests()
    {
        _cacheServiceMock = new Mock<ICacheService>();
        _cestaCacheServiceMock = new Mock<CestaCacheService>(_cacheServiceMock.Object);
        _cotacaoCacheServiceMock = new Mock<CotacaoCacheService>(_cacheServiceMock.Object);
        _cotahistParserMock = new Mock<CotahistParser>(_cestaCacheServiceMock.Object, _cotacaoCacheServiceMock.Object);
        _cotacaoService = new CotacaoService(_cotahistParserMock.Object, _pastaCotacoes);
    }

    [Fact]
    public async Task ObterCotacaoAtualAsync_ComTickerValido_DeveRetornarCotacao()
    {
        // Arrange
        var ticker = new Ticker("PETR4");
        var cotacaoEsperada = new CotacaoB3
        {
            Ticker = "PETR4",
            PrecoFechamento = 35.50m
        };

        _cotahistParserMock
            .Setup(x => x.ObterCotacaoFechamento(_pastaCotacoes, ticker))
            .Returns(cotacaoEsperada);

        // Act
        var resultado = await _cotacaoService.ObterCotacaoAtualAsync(ticker);

        // Assert
        resultado.Should().Be(35.50m);
        _cotahistParserMock.Verify(x => x.ObterCotacaoFechamento(_pastaCotacoes, ticker), Times.Once);
    }

    [Fact]
    public async Task ObterCotacaoAtualAsync_SemCotacao_DeveRetornarZero()
    {
        // Arrange
        var ticker = new Ticker("VALE3");

        _cotahistParserMock
            .Setup(x => x.ObterCotacaoFechamento(_pastaCotacoes, ticker))
            .Returns((CotacaoB3?)null);

        // Act
        var resultado = await _cotacaoService.ObterCotacaoAtualAsync(ticker);

        // Assert
        resultado.Should().Be(0m);
        _cotahistParserMock.Verify(x => x.ObterCotacaoFechamento(_pastaCotacoes, ticker), Times.Once);
    }

    [Fact]
    public async Task ObterCotacoesAsync_ComMultiplosTickers_DeveRetornarDicionario()
    {
        // Arrange
        var tickers = new List<Ticker>
        {
            new("PETR4"),
            new("VALE3"),
            new("ITUB4")
        };

        var cotacoesMock = new Dictionary<Ticker, CotacaoB3?>
        {
            [tickers[0]] = new CotacaoB3 { Ticker = "PETR4", PrecoFechamento = 35.50m },
            [tickers[1]] = new CotacaoB3 { Ticker = "VALE3", PrecoFechamento = 68.75m },
            [tickers[2]] = null
        };

        foreach (var kvp in cotacoesMock)
        {
            _cotahistParserMock
                .Setup(x => x.ObterCotacaoFechamento(_pastaCotacoes, kvp.Key))
                .Returns(kvp.Value);
        }

        // Act
        var resultado = await _cotacaoService.ObterCotacoesAsync(tickers);

        // Assert
        resultado.Should().HaveCount(3);
        resultado[tickers[0]].Should().Be(35.50m);
        resultado[tickers[1]].Should().Be(68.75m);
        resultado[tickers[2]].Should().Be(0m);

        foreach (var ticker in tickers)
        {
            _cotahistParserMock.Verify(x => x.ObterCotacaoFechamento(_pastaCotacoes, ticker), Times.Once);
        }
    }

    [Fact]
    public async Task ObterCotacoesAsync_ComListaVazia_DeveRetornarDicionarioVazio()
    {
        // Arrange
        var tickers = new List<Ticker>();

        // Act
        var resultado = await _cotacaoService.ObterCotacoesAsync(tickers);

        // Assert
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task ExisteCotacaoAsync_ComArquivoInexistente_DeveRetornarFalse()
    {
        // Arrange
        var ticker = new Ticker("PETR4");
        var data = new DateTime(2024, 01, 15);

        // Act
        var resultado = await _cotacaoService.ExisteCotacaoAsync(ticker, data);

        // Assert
        resultado.Should().BeFalse();
    }
}
