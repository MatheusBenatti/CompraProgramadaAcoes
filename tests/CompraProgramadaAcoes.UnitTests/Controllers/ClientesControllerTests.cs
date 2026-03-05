using CompraProgramadaAcoes.Api.Controllers;
using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Application.Exceptions;
using CompraProgramadaAcoes.Application.Interfaces.UseCases;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CompraProgramadaAcoes.UnitTests.Controllers;

public class ClientesControllerTests
{
    private readonly Mock<IRealizarAdesao> _realizarAdesaoMock;
    private readonly Mock<IRealizarSaida> _realizarSaidaMock;
    private readonly Mock<IAlterarValorMensal> _alterarValorMensalMock;
    private readonly Mock<IConsultarCarteira> _consultarCarteiraMock;
    private readonly Mock<IConsultarRentabilidade> _consultarRentabilidadeMock;
    private readonly ClientesController _controller;

    public ClientesControllerTests()
    {
        _realizarAdesaoMock = new Mock<IRealizarAdesao>();
        _realizarSaidaMock = new Mock<IRealizarSaida>();
        _alterarValorMensalMock = new Mock<IAlterarValorMensal>();
        _consultarCarteiraMock = new Mock<IConsultarCarteira>();
        _consultarRentabilidadeMock = new Mock<IConsultarRentabilidade>();
        _controller = new ClientesController(
            _realizarAdesaoMock.Object,
            _realizarSaidaMock.Object,
            _alterarValorMensalMock.Object,
            _consultarCarteiraMock.Object,
            _consultarRentabilidadeMock.Object);
    }

    [Fact]
    public async Task Adesao_ComDadosValidos_DeveRetornarCreated()
    {
        // Arrange
        var request = new AdesaoRequest
        {
            Nome = "João Silva",
            Cpf = "12345678901",
            Email = "joao@teste.com",
            ValorMensal = 500
        };

        var response = new AdesaoResponse
        {
            ClienteId = 1,
            Nome = request.Nome,
            Cpf = request.Cpf,
            Email = request.Email,
            ValorMensal = request.ValorMensal,
            Ativo = true
        };

        _realizarAdesaoMock
            .Setup(x => x.ExecuteAsync(request))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Adesao(request);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.Value.Should().BeEquivalentTo(response);
        createdResult.ActionName.Should().Be(nameof(ClientesController.Adesao));
        createdResult.RouteValues!["id"].Should().Be(response.ClienteId);
    }

    [Fact]
    public async Task Adesao_ComModelInvalido_DeveRetornarBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("Nome", "Required");
        var request = new AdesaoRequest();

        // Act
        var result = await _controller.Adesao(request);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        
        var errorResponse = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Erro.Should().Be("Dados inválidos.");
        errorResponse.Codigo.Should().Be("REQUISICAO_INVALIDA");
    }

    [Fact]
    public async Task Adesao_ComCpfDuplicado_DeveRetornarBadRequest()
    {
        // Arrange
        var request = new AdesaoRequest
        {
            Nome = "João Silva",
            Cpf = "12345678901",
            Email = "joao@teste.com",
            ValorMensal = 500
        };

        _realizarAdesaoMock
            .Setup(x => x.ExecuteAsync(request))
            .ThrowsAsync(new ClienteCpfDuplicadoException("CPF ja cadastrado no sistema."));

        // Act
        var result = await _controller.Adesao(request);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        
        var errorResponse = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Erro.Should().Be("CPF ja cadastrado no sistema.");
        errorResponse.Codigo.Should().Be("CLIENTE_CPF_DUPLICADO");
    }

    [Fact]
    public async Task Adesao_ComArgumentException_DeveRetornarBadRequest()
    {
        // Arrange
        var request = new AdesaoRequest
        {
            Nome = "João Silva",
            Cpf = "12345678901",
            Email = "joao@teste.com",
            ValorMensal = 500
        };

        _realizarAdesaoMock
            .Setup(x => x.ExecuteAsync(request))
            .ThrowsAsync(new ArgumentException("Nome inválido"));

        // Act
        var result = await _controller.Adesao(request);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        
        var errorResponse = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Erro.Should().Be("Nome inválido");
        errorResponse.Codigo.Should().Be("VALIDACAO_ERRO");
    }

    [Fact]
    public async Task Adesao_ComExcecaoGenerica_DeveRetornarInternalServerError()
    {
        // Arrange
        var request = new AdesaoRequest
        {
            Nome = "João Silva",
            Cpf = "12345678901",
            Email = "joao@teste.com",
            ValorMensal = 500
        };

        _realizarAdesaoMock
            .Setup(x => x.ExecuteAsync(request))
            .ThrowsAsync(new Exception("Erro inesperado"));

        // Act
        var result = await _controller.Adesao(request);

        // Assert
        var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusCodeResult.StatusCode.Should().Be(500);
        
        var errorResponse = statusCodeResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Erro.Should().Be("Erro interno ao processar adesão.");
        errorResponse.Codigo.Should().Be("ERRO_INTERNO");
    }

    [Fact]
    public async Task Saida_ComClienteExistente_DeveRetornarOk()
    {
        // Arrange
        var clienteId = 1;
        var response = new SaidaResponse
        {
            ClienteId = clienteId,
            Nome = "João Silva",
            Ativo = false,
            DataSaida = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            Mensagem = "Saída realizada com sucesso."
        };

        _realizarSaidaMock
            .Setup(x => x.ExecuteAsync(clienteId))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Saida(clienteId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task Saida_ComClienteNaoEncontrado_DeveRetornarNotFound()
    {
        // Arrange
        var clienteId = 999;

        _realizarSaidaMock
            .Setup(x => x.ExecuteAsync(clienteId))
            .ThrowsAsync(new ClienteNaoEncontradoException());

        // Act
        var result = await _controller.Saida(clienteId);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
        
        var errorResponse = notFoundResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Erro.Should().Be("Cliente não encontrado.");
        errorResponse.Codigo.Should().Be("CLIENTE_NAO_ENCONTRADO");
    }

    [Fact]
    public async Task Saida_ComClienteJaInativo_DeveRetornarBadRequest()
    {
        // Arrange
        var clienteId = 1;

        _realizarSaidaMock
            .Setup(x => x.ExecuteAsync(clienteId))
            .ThrowsAsync(new ClienteJaInativoException());

        // Act
        var result = await _controller.Saida(clienteId);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        
        var errorResponse = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Erro.Should().Be("Cliente já havia saído do produto.");
        errorResponse.Codigo.Should().Be("CLIENTE_JA_INATIVO");
    }

    [Fact]
    public async Task AlterarValorMensal_ComDadosValidos_DeveRetornarOk()
    {
        // Arrange
        var clienteId = 1;
        var request = new AlterarValorMensalRequest
        {
            NovoValorMensal = 1000
        };

        var response = new AlterarValorMensalResponse
        {
            ClienteId = clienteId,
            ValorMensalAnterior = 500,
            ValorMensalNovo = request.NovoValorMensal,
            DataAlteracao = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            Mensagem = "Valor mensal alterado com sucesso."
        };

        _alterarValorMensalMock
            .Setup(x => x.ExecuteAsync(clienteId, request))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.AlterarValorMensal(clienteId, request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task AlterarValorMensal_ComModelInvalido_DeveRetornarBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("NovoValorMensal", "Required");
        var clienteId = 1;
        var request = new AlterarValorMensalRequest();

        // Act
        var result = await _controller.AlterarValorMensal(clienteId, request);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        
        var errorResponse = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Erro.Should().Be("Dados inválidos.");
        errorResponse.Codigo.Should().Be("REQUISICAO_INVALIDA");
    }

    [Fact]
    public async Task AlterarValorMensal_ComClienteNaoEncontrado_DeveRetornarNotFound()
    {
        // Arrange
        var clienteId = 999;
        var request = new AlterarValorMensalRequest
        {
            NovoValorMensal = 1000
        };

        _alterarValorMensalMock
            .Setup(x => x.ExecuteAsync(clienteId, request))
            .ThrowsAsync(new ClienteNaoEncontradoException());

        // Act
        var result = await _controller.AlterarValorMensal(clienteId, request);

        // Assert
        var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.StatusCode.Should().Be(404);
        
        var errorResponse = notFoundResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Erro.Should().Be("Cliente não encontrado.");
        errorResponse.Codigo.Should().Be("CLIENTE_NAO_ENCONTRADO");
    }

    [Fact]
    public async Task AlterarValorMensal_ComClienteJaInativo_DeveRetornarBadRequest()
    {
        // Arrange
        var clienteId = 1;
        var request = new AlterarValorMensalRequest
        {
            NovoValorMensal = 1000
        };

        _alterarValorMensalMock
            .Setup(x => x.ExecuteAsync(clienteId, request))
            .ThrowsAsync(new ClienteJaInativoException());

        // Act
        var result = await _controller.AlterarValorMensal(clienteId, request);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequestResult.StatusCode.Should().Be(400);
        
        var errorResponse = badRequestResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        errorResponse.Erro.Should().Be("Cliente já havia saído do produto.");
        errorResponse.Codigo.Should().Be("CLIENTE_JA_INATIVO");
    }
}
