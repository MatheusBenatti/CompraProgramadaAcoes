using CompraProgramadaAcoes.Api.Controllers;
using CompraProgramadaAcoes.Application.Interfaces;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Application.Services;
using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Moq;

namespace CompraProgramadaAcoes.UnitTests.Controllers;

public class AdminControllerTests
{
    private readonly Mock<ICestaRecomendacaoRepository> _cestaRepositoryMock;
    private readonly Mock<IContaGraficaRepository> _contaGraficaRepositoryMock;
    private readonly Mock<ICustodiaRepository> _custodiaRepositoryMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ICestaCacheService> _cestaCacheServiceMock;
    private readonly Mock<CotacaoCacheService> _cotacaoCacheServiceMock;
    private readonly Mock<CotahistParser> _cotahistParserMock;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ICotacaoRepository> _cotacaoRepositoryMock;
    private readonly AdminController _controller;

    public AdminControllerTests()
    {
        _cestaRepositoryMock = new Mock<ICestaRecomendacaoRepository>();
        _contaGraficaRepositoryMock = new Mock<IContaGraficaRepository>();
        _custodiaRepositoryMock = new Mock<ICustodiaRepository>();
        _cacheServiceMock = new Mock<ICacheService>();
        _cestaCacheServiceMock = new Mock<ICestaCacheService>();
        _cotacaoCacheServiceMock = new Mock<CotacaoCacheService>(_cacheServiceMock.Object);
        _cotacaoRepositoryMock = new Mock<ICotacaoRepository>();
        _cotahistParserMock = new Mock<CotahistParser>(_cestaCacheServiceMock.Object, _cotacaoCacheServiceMock.Object, _cotacaoRepositoryMock.Object);
        _envMock = new Mock<IWebHostEnvironment>();
        _configurationMock = new Mock<IConfiguration>();
        
        // Configurar o mock para retornar o caminho padrão
        _configurationMock.Setup(c => c["FileStorage:CotacoesPath"]).Returns("cotacoes");
        _envMock.Setup(e => e.ContentRootPath).Returns(Directory.GetCurrentDirectory());
        
        _controller = new AdminController(
            _cestaRepositoryMock.Object,
            _contaGraficaRepositoryMock.Object,
            _custodiaRepositoryMock.Object,
            _cotahistParserMock.Object,
            _cestaCacheServiceMock.Object,
            _envMock.Object,
            _configurationMock.Object);
    }

    [Fact]
    public async Task CadastrarCesta_ComDadosValidos_DeveRetornarCreated()
    {
        // Arrange
        var request = new CadastrarCestaAdminRequest
        {
            Nome = "Cesta Top Five",
            Itens = new List<CadastrarCestaItemRequest>
            {
                new() { Ticker = "PETR4", Percentual = 20 },
                new() { Ticker = "VALE3", Percentual = 20 },
                new() { Ticker = "ITUB4", Percentual = 20 },
                new() { Ticker = "BBDC4", Percentual = 20 },
                new() { Ticker = "WEGE3", Percentual = 20 }
            }
        };

        _cestaRepositoryMock
            .Setup(x => x.ObterCestaVigenteAsync())
            .ReturnsAsync((CestaRecomendacao?)null);

        _cestaRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<CestaRecomendacao>()))
            .Returns(Task.CompletedTask);

        _cestaRepositoryMock
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        _cestaCacheServiceMock
            .Setup(x => x.SalvarCestaAsync(It.IsAny<CestaCacheDTO>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CadastrarCesta(request);

        // Assert
        var createdResult = result.Should().BeOfType<ActionResult<CestaAdminResponse>>().Subject;
        var createdAtActionResult = createdResult.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdAtActionResult.StatusCode.Should().Be(201);
        createdAtActionResult.ActionName.Should().Be(nameof(AdminController.ObterCestaAtual));

        var response = createdAtActionResult.Value.Should().BeAssignableTo<CestaAdminResponse>().Subject;
        response.Nome.Should().Be(request.Nome);
        response.Itens.Should().HaveCount(5);
        response.RebalanceamentoDisparado.Should().BeFalse();
        response.Mensagem.Should().Be("Primeira cesta cadastrada com sucesso.");
    }

    [Fact]
    public async Task CadastrarCesta_ComCestaExistente_DeveRetornarCreatedComRebalanceamento()
    {
        // Arrange
        var cestaExistente = new CestaRecomendacao();
        cestaExistente.AtualizarNome("Cesta Antiga");
        cestaExistente.AdicionarItem("PETR4", 20);
        cestaExistente.AdicionarItem("VALE3", 20);
        cestaExistente.AdicionarItem("ITUB4", 20);
        cestaExistente.AdicionarItem("BBDC4", 20);
        cestaExistente.AdicionarItem("WEGE3", 20);

        var request = new CadastrarCestaAdminRequest
        {
            Nome = "Cesta Nova",
            Itens = new List<CadastrarCestaItemRequest>
            {
                new() { Ticker = "PETR4", Percentual = 25 },
                new() { Ticker = "VALE3", Percentual = 25 },
                new() { Ticker = "ITUB4", Percentual = 25 },
                new() { Ticker = "BBDC4", Percentual = 25 },
                new() { Ticker = "MGLU3", Percentual = 0 }
            }
        };

        _cestaRepositoryMock
            .Setup(x => x.ObterCestaVigenteAsync())
            .ReturnsAsync(cestaExistente);

        _cestaRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<CestaRecomendacao>()))
            .Returns(Task.CompletedTask);

        _cestaRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<CestaRecomendacao>()))
            .Returns(Task.CompletedTask);

        _cestaRepositoryMock
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        _cestaCacheServiceMock
            .Setup(x => x.SalvarCestaAsync(It.IsAny<CestaCacheDTO>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CadastrarCesta(request);

        // Assert
        var createdResult = result.Should().BeOfType<ActionResult<CestaAdminResponse>>().Subject;
        var createdAtActionResult = createdResult.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdAtActionResult.StatusCode.Should().Be(201);

        var response = createdAtActionResult.Value.Should().BeAssignableTo<CestaAdminResponse>().Subject;
        response.Nome.Should().Be(request.Nome);
        response.RebalanceamentoDisparado.Should().BeTrue();
        response.Mensagem.Should().Contain("Cesta atualizada");
        response.Mensagem.Should().Contain("[WEGE3]");
        response.Mensagem.Should().Contain("[MGLU3]");
    }

    [Fact]
    public async Task CadastrarCesta_ComQuantidadeInvalida_DeveRetornarBadRequest()
    {
        // Arrange
        var request = new CadastrarCestaAdminRequest
        {
            Nome = "Cesta Invalida",
            Itens = new List<CadastrarCestaItemRequest>
            {
                new() { Ticker = "PETR4", Percentual = 50 },
                new() { Ticker = "VALE3", Percentual = 50 }
            }
        };

        _cestaRepositoryMock
            .Setup(x => x.ObterCestaVigenteAsync())
            .ReturnsAsync((CestaRecomendacao?)null);

        // Act
        var result = await _controller.CadastrarCesta(request);

        // Assert
        var badRequestResult = result.Should().BeOfType<ActionResult<CestaAdminResponse>>().Subject;
        var badRequestObjectResult = badRequestResult.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestObjectResult.StatusCode.Should().Be(400);

        var errorResponse = badRequestObjectResult.Value.Should().BeAssignableTo<object>().Subject;
        var erroProp = errorResponse.GetType().GetProperty("Erro");
        erroProp?.GetValue(errorResponse)?.ToString().Should().Contain("exatamente 5 ativos");
    }

    [Fact]
    public async Task CadastrarCesta_ComPercentuaisInvalidos_DeveRetornarBadRequest()
    {
        // Arrange
        var request = new CadastrarCestaAdminRequest
        {
            Nome = "Cesta Invalida",
            Itens = new List<CadastrarCestaItemRequest>
            {
                new() { Ticker = "PETR4", Percentual = 30 },
                new() { Ticker = "VALE3", Percentual = 30 },
                new() { Ticker = "ITUB4", Percentual = 20 },
                new() { Ticker = "BBDC4", Percentual = 10 },
                new() { Ticker = "WEGE3", Percentual = 5 }
            }
        };

        _cestaRepositoryMock
            .Setup(x => x.ObterCestaVigenteAsync())
            .ReturnsAsync((CestaRecomendacao?)null);

        // Act
        var result = await _controller.CadastrarCesta(request);

        // Assert
        var badRequestResult = result.Should().BeOfType<ActionResult<CestaAdminResponse>>().Subject;
        var badRequestObjectResult = badRequestResult.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestObjectResult.StatusCode.Should().Be(400);

        var errorResponse = badRequestObjectResult.Value.Should().BeAssignableTo<object>().Subject;
        var erroProp = errorResponse.GetType().GetProperty("Erro");
        erroProp?.GetValue(errorResponse)?.ToString().Should().Contain("exatamente 100%");
    }

    [Fact]
    public async Task ObterCestaAtual_ComCestaExistente_DeveRetornarOk()
    {
        // Arrange
        var cesta = new CestaRecomendacao();
        cesta.AtualizarNome("Cesta Atual");
        cesta.AdicionarItem("PETR4", 20);
        cesta.AdicionarItem("VALE3", 20);
        cesta.AdicionarItem("ITUB4", 20);
        cesta.AdicionarItem("BBDC4", 20);
        cesta.AdicionarItem("WEGE3", 20);

        var cotacoes = new Dictionary<string, Cotacao>
        {
            ["PETR4"] = new Cotacao { Ticker = "PETR4", PrecoFechamento = 35.50m },
            ["VALE3"] = new Cotacao { Ticker = "VALE3", PrecoFechamento = 68.75m },
            ["ITUB4"] = new Cotacao { Ticker = "ITUB4", PrecoFechamento = 31.25m },
            ["BBDC4"] = new Cotacao { Ticker = "BBDC4", PrecoFechamento = 18.45m },
            ["WEGE3"] = new Cotacao { Ticker = "WEGE3", PrecoFechamento = 52.30m }
        };

        _cestaRepositoryMock
            .Setup(x => x.ObterCestaVigenteAsync())
            .ReturnsAsync(cesta);

        _cotahistParserMock
            .Setup(x => x.ObterCotacoesFechamento(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
            .Returns(cotacoes);

        // Act
        var result = await _controller.ObterCestaAtual();

        // Assert
        var okResult = result.Should().BeOfType<ActionResult<CestaAtualResponse>>().Subject;
        var okObjectResult = okResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        okObjectResult.StatusCode.Should().Be(200);

        var response = okObjectResult.Value.Should().BeAssignableTo<CestaAtualResponse>().Subject;
        response.Nome.Should().Be("Cesta Atual");
        response.Itens.Should().HaveCount(5);
        response.Itens.First(i => i.Ticker == "PETR4").CotacaoAtual.Should().Be(35.50m);
        response.Itens.First(i => i.Ticker == "VALE3").CotacaoAtual.Should().Be(68.75m);
    }

    [Fact]
    public async Task ObterCestaAtual_SemCesta_DeveRetornarNotFound()
    {
        // Arrange
        _cestaRepositoryMock
            .Setup(x => x.ObterCestaVigenteAsync())
            .ReturnsAsync((CestaRecomendacao?)null);

        // Act
        var result = await _controller.ObterCestaAtual();

        // Assert
        var notFoundResult = result.Should().BeOfType<ActionResult<CestaAtualResponse>>().Subject;
        var notFoundObjectResult = notFoundResult.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundObjectResult.StatusCode.Should().Be(404);

        var errorResponse = notFoundObjectResult.Value.Should().BeAssignableTo<object>().Subject;
        var erroProp = errorResponse.GetType().GetProperty("Erro");
        erroProp?.GetValue(errorResponse)?.ToString().Should().Be("Nenhuma cesta ativa encontrada.");
    }

    [Fact]
    public async Task ObterHistoricoCestas_ComCestas_DeveRetornarOk()
    {
        // Arrange
        var cestas = new List<CestaRecomendacao>
        {
            CreateCestaTest(1, "Cesta 1", true),
            CreateCestaTest(2, "Cesta 2", false)
        };

        _cestaRepositoryMock
            .Setup(x => x.ObterHistoricoAsync())
            .ReturnsAsync(cestas);

        // Act
        var result = await _controller.ObterHistoricoCestas();

        // Assert
        var okResult = result.Should().BeOfType<ActionResult<CestasHistoricoResponse>>().Subject;
        var okObjectResult = okResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        okObjectResult.StatusCode.Should().Be(200);

        var response = okObjectResult.Value.Should().BeAssignableTo<CestasHistoricoResponse>().Subject;
        response.Cestas.Should().HaveCount(2);
        response.Cestas[0].Nome.Should().Be("Cesta 1");
        response.Cestas[1].Nome.Should().Be("Cesta 2");
    }

    [Fact]
    public async Task ObterCustodiaMaster_ComContaExistente_DeveRetornarOk()
    {
        // Arrange
        var contaMaster = new ContaGrafica(0);
        contaMaster.GetType().GetProperty(nameof(ContaGrafica.Id))?.SetValue(contaMaster, 1L);
        contaMaster.GetType().GetProperty(nameof(ContaGrafica.Tipo))?.SetValue(contaMaster, "MASTER");
        contaMaster.GerarNumeroConta();

        var custodia = new List<Custodia>
        {
            new(contaMaster.Id),
            new(contaMaster.Id)
        };
        custodia[0].AtualizarCustodia(100, 30.00m, "PETR4");
        custodia[1].AtualizarCustodia(50, 65.00m, "VALE3");

        var cotacoes = new Dictionary<string, Cotacao>
        {
            ["PETR4"] = new Cotacao { Ticker = "PETR4", PrecoFechamento = 35.50m },
            ["VALE3"] = new Cotacao { Ticker = "VALE3", PrecoFechamento = 68.75m }
        };

        _contaGraficaRepositoryMock
            .Setup(x => x.ObterPorTipoAsync("MASTER"))
            .ReturnsAsync(new List<ContaGrafica> { contaMaster });

        _custodiaRepositoryMock
            .Setup(x => x.ObterPorContaGraficaAsync(contaMaster.Id))
            .ReturnsAsync(custodia);

        _cotahistParserMock
            .Setup(x => x.ObterCotacoesFechamento(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
            .Returns(cotacoes);

        // Act
        var result = await _controller.ObterCustodiaMaster();

        // Assert
        var okResult = result.Should().BeOfType<ActionResult<ContaMasterCustodiaResponse>>().Subject;
        var okObjectResult = okResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        okObjectResult.StatusCode.Should().Be(200);

        var response = okObjectResult.Value.Should().BeAssignableTo<ContaMasterCustodiaResponse>().Subject;
        response.ContaMaster.Id.Should().Be(1L);
        response.Custodia.Should().HaveCount(2);
        response.ValorTotalResiduo.Should().Be((35.50m * 100) + (68.75m * 50));
    }

    [Fact]
    public async Task ObterCustodiaMaster_SemContaMaster_DeveRetornarNotFound()
    {
        // Arrange
        _contaGraficaRepositoryMock
            .Setup(x => x.ObterPorTipoAsync("MASTER"))
            .ReturnsAsync(new List<ContaGrafica>());

        // Act
        var result = await _controller.ObterCustodiaMaster();

        // Assert
        var notFoundResult = result.Should().BeOfType<ActionResult<ContaMasterCustodiaResponse>>().Subject;
        var notFoundObjectResult = notFoundResult.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundObjectResult.StatusCode.Should().Be(404);
        notFoundObjectResult.Value.Should().Be("Conta master não encontrada");
    }

    private static CestaRecomendacao CreateCestaTest(long id, string nome, bool ativa)
    {
        var cesta = new CestaRecomendacao();
        cesta.GetType().GetProperty(nameof(CestaRecomendacao.Id))?.SetValue(cesta, id);
        cesta.AtualizarNome(nome);
        cesta.AdicionarItem("PETR4", 20);
        cesta.AdicionarItem("VALE3", 20);
        cesta.AdicionarItem("ITUB4", 20);
        cesta.AdicionarItem("BBDC4", 20);
        cesta.AdicionarItem("WEGE3", 20);
        
        if (!ativa)
        {
            cesta.Desativar();
        }
        
        return cesta;
    }
}
