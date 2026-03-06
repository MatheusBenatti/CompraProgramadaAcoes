using CompraProgramadaAcoes.Api.Controllers;
using CompraProgramadaAcoes.Application.Interfaces.Services;
using CompraProgramadaAcoes.Application.DTOs.Admin;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CompraProgramadaAcoes.UnitTests.Controllers;

public class AdminControllerTests
{
    private readonly Mock<IAdminService> _adminServiceMock;
    private readonly AdminController _controller;

    public AdminControllerTests()
    {
        _adminServiceMock = new Mock<IAdminService>();
        _controller = new AdminController(_adminServiceMock.Object);
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

        var expectedResponse = new CestaAdminResponse
        {
            CestaId = 1,
            Nome = "Cesta Top Five",
            Ativa = true,
            DataCriacao = DateTime.UtcNow,
            Itens = new List<CestaItemAdminResponse>
            {
                new() { Ticker = "PETR4", Percentual = 20 },
                new() { Ticker = "VALE3", Percentual = 20 },
                new() { Ticker = "ITUB4", Percentual = 20 },
                new() { Ticker = "BBDC4", Percentual = 20 },
                new() { Ticker = "WEGE3", Percentual = 20 }
            },
            Mensagem = "Cesta cadastrada com sucesso"
        };

        _adminServiceMock
            .Setup(x => x.CadastrarCestaAsync(request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CadastrarCesta(request);

        // Assert
        var createdResult = result.Should().BeOfType<ActionResult<CestaAdminResponse>>().Subject;
        var createdAtActionResult = createdResult.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdAtActionResult.StatusCode.Should().Be(201);
        createdAtActionResult.ActionName.Should().Be(nameof(AdminController.ObterCestaAtual));
        
        var responseValue = createdAtActionResult.Value.Should().BeOfType<CestaAdminResponse>().Subject;
        responseValue.Nome.Should().Be("Cesta Top Five");
        responseValue.Itens.Should().HaveCount(5);
    }

    [Fact]
    public async Task ObterCestaAtual_ComCestaExistente_DeveRetornarOk()
    {
        // Arrange
        var expectedResponse = new CestaAtualResponse
        {
            CestaId = 1,
            Nome = "Cesta Top Five",
            Ativa = true,
            DataCriacao = DateTime.UtcNow,
            Itens = new List<CestaItemAtualResponse>
            {
                new() { Ticker = "PETR4", Percentual = 20, CotacaoAtual = 30.50m },
                new() { Ticker = "VALE3", Percentual = 20, CotacaoAtual = 70.25m }
            }
        };

        _adminServiceMock
            .Setup(x => x.ObterCestaAtualAsync())
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ObterCestaAtual();

        // Assert
        var okResult = result.Should().BeOfType<ActionResult<CestaAtualResponse>>().Subject;
        var actionResult = okResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        actionResult.StatusCode.Should().Be(200);
        
        var responseValue = actionResult.Value.Should().BeOfType<CestaAtualResponse>().Subject;
        responseValue.Nome.Should().Be("Cesta Top Five");
        responseValue.Itens.Should().HaveCount(2);
    }

    [Fact]
    public async Task ObterHistoricoCestas_ComCestasExistentes_DeveRetornarOk()
    {
        // Arrange
        var expectedResponse = new CestasHistoricoResponse
        {
            Cestas = new List<CestaHistoricoItemResponse>
            {
                new() 
                { 
                    CestaId = 1, 
                    Nome = "Cesta Top Five V1", 
                    Ativa = false, 
                    DataCriacao = DateTime.UtcNow.AddDays(-30),
                    DataDesativacao = DateTime.UtcNow.AddDays(-15)
                },
                new() 
                { 
                    CestaId = 2, 
                    Nome = "Cesta Top Five V2", 
                    Ativa = true, 
                    DataCriacao = DateTime.UtcNow.AddDays(-14)
                }
            }
        };

        _adminServiceMock
            .Setup(x => x.ObterHistoricoCestasAsync())
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ObterHistoricoCestas();

        // Assert
        var okResult = result.Should().BeOfType<ActionResult<CestasHistoricoResponse>>().Subject;
        var actionResult = okResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        actionResult.StatusCode.Should().Be(200);
        
        var responseValue = actionResult.Value.Should().BeOfType<CestasHistoricoResponse>().Subject;
        responseValue.Cestas.Should().HaveCount(2);
    }

    [Fact]
    public async Task ObterCustodiaMaster_ComCustodiaExistente_DeveRetornarOk()
    {
        // Arrange
        var expectedResponse = new ContaMasterCustodiaResponse
        {
            ContaMaster = new ContaMasterInfoResponse
            {
                Id = 1,
                NumeroConta = "MASTER-001",
                Tipo = "MASTER"
            },
            Custodia = new List<ContaMasterCustodiaItemResponse>
            {
                new() { Ticker = "PETR4", Quantidade = 100, PrecoMedio = 30.50m, ValorAtual = 3050m, Origem = "Resíduo" },
                new() { Ticker = "VALE3", Quantidade = 50, PrecoMedio = 70.25m, ValorAtual = 3512.50m, Origem = "Resíduo" }
            },
            ValorTotalResiduo = 6562.50m
        };

        _adminServiceMock
            .Setup(x => x.ObterCustodiaMasterAsync())
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ObterCustodiaMaster();

        // Assert
        var okResult = result.Should().BeOfType<ActionResult<ContaMasterCustodiaResponse>>().Subject;
        var actionResult = okResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        actionResult.StatusCode.Should().Be(200);
        
        var responseValue = actionResult.Value.Should().BeOfType<ContaMasterCustodiaResponse>().Subject;
        responseValue.ContaMaster.NumeroConta.Should().Be("MASTER-001");
        responseValue.Custodia.Should().HaveCount(2);
        responseValue.ValorTotalResiduo.Should().Be(6562.50m);
    }
}
