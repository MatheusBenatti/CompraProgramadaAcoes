using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Application.Exceptions;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Application.UseCases;
using CompraProgramadaAcoes.Domain.Entities;
using CompraProgramadaAcoes.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CompraProgramadaAcoes.UnitTests.UseCases;

public class RealizarAdesaoTests
{
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<IContaGraficaRepository> _contaGraficaRepositoryMock;
    private readonly Mock<ICustodiaRepository> _custodiaRepositoryMock;
    private readonly Mock<IClienteFactory> _clienteFactoryMock;
    private readonly Mock<IContaGraficaFactory> _contaGraficaFactoryMock;
    private readonly Mock<ICustodiaFactory> _custodiaFactoryMock;
    private readonly RealizarAdesao _realizarAdesao;

    public RealizarAdesaoTests()
    {
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _contaGraficaRepositoryMock = new Mock<IContaGraficaRepository>();
        _custodiaRepositoryMock = new Mock<ICustodiaRepository>();
        _clienteFactoryMock = new Mock<IClienteFactory>();
        _contaGraficaFactoryMock = new Mock<IContaGraficaFactory>();
        _custodiaFactoryMock = new Mock<ICustodiaFactory>();

        _realizarAdesao = new RealizarAdesao(
            _clienteRepositoryMock.Object,
            _contaGraficaRepositoryMock.Object,
            _custodiaRepositoryMock.Object,
            _clienteFactoryMock.Object,
            _contaGraficaFactoryMock.Object,
            _custodiaFactoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_QuandoCpfJaExiste_DeveLancarClienteCpfDuplicadoException()
    {
        // Arrange
        var request = new AdesaoRequest
        {
            Nome = "João Silva",
            Cpf = "12345678901",
            Email = "joao@teste.com",
            ValorMensal = 500
        };

        _clienteRepositoryMock
            .Setup(x => x.CpfExistsAsync(request.Cpf))
            .ReturnsAsync(true);

        // Act & Assert
        await _realizarAdesao
            .Invoking(x => x.ExecuteAsync(request))
            .Should()
            .ThrowAsync<ClienteCpfDuplicadoException>()
            .WithMessage("CPF ja cadastrado no sistema.");

        _clienteRepositoryMock.Verify(x => x.CpfExistsAsync(request.Cpf), Times.Once);
        _clienteFactoryMock.Verify(x => x.Criar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_QuandoCpfNaoExiste_DeveRealizarAdesaoComSucesso()
    {
        // Arrange
        var request = new AdesaoRequest
        {
            Nome = "Maria Santos",
            Cpf = "98765432100",
            Email = "maria@teste.com",
            ValorMensal = 1000
        };

        var cliente = new Cliente(request.Nome, request.Cpf, request.Email, request.ValorMensal);
        cliente.GetType().GetProperty(nameof(Cliente.Id))?.SetValue(cliente, 1L);

        var contaGrafica = new ContaGrafica(cliente.Id);
        contaGrafica.GetType().GetProperty(nameof(ContaGrafica.Id))?.SetValue(contaGrafica, 1L);
        contaGrafica.GerarNumeroConta();

        var custodia = new Custodia(contaGrafica.Id);

        _clienteRepositoryMock
            .Setup(x => x.CpfExistsAsync(request.Cpf))
            .ReturnsAsync(false);

        _clienteFactoryMock
            .Setup(x => x.Criar(request.Nome, request.Cpf, request.Email, request.ValorMensal))
            .Returns(cliente);

        _clienteRepositoryMock
            .Setup(x => x.AddAsync(cliente))
            .ReturnsAsync(cliente);

        _contaGraficaFactoryMock
            .Setup(x => x.Criar(cliente.Id))
            .Returns(contaGrafica);

        _contaGraficaRepositoryMock
            .Setup(x => x.AddAsync(contaGrafica))
            .ReturnsAsync(contaGrafica);

        _custodiaFactoryMock
            .Setup(x => x.Criar(contaGrafica.Id))
            .Returns(custodia);

        _custodiaRepositoryMock
            .Setup(x => x.AddAsync(custodia))
            .ReturnsAsync(custodia);

        // Act
        var result = await _realizarAdesao.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.ClienteId.Should().Be(cliente.Id);
        result.Nome.Should().Be(cliente.Nome);
        result.Cpf.Should().Be(cliente.Cpf);
        result.Email.Should().Be(cliente.Email);
        result.ValorMensal.Should().Be(cliente.ValorMensal);
        result.Ativo.Should().BeTrue();
        result.ContaGrafica.Should().NotBeNull();
        result.ContaGrafica.Id.Should().Be(contaGrafica.Id);
        result.ContaGrafica.NumeroConta.Should().Be(contaGrafica.NumeroConta);
        result.ContaGrafica.Tipo.Should().Be(contaGrafica.Tipo);

        _clienteRepositoryMock.Verify(x => x.CpfExistsAsync(request.Cpf), Times.Once);
        _clienteFactoryMock.Verify(x => x.Criar(request.Nome, request.Cpf, request.Email, request.ValorMensal), Times.Once);
        _clienteRepositoryMock.Verify(x => x.AddAsync(cliente), Times.Once);
        _clienteRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));

        _contaGraficaFactoryMock.Verify(x => x.Criar(cliente.Id), Times.Once);
        _contaGraficaRepositoryMock.Verify(x => x.AddAsync(contaGrafica), Times.Once);
        _contaGraficaRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));

        _custodiaFactoryMock.Verify(x => x.Criar(contaGrafica.Id), Times.Once);
        _custodiaRepositoryMock.Verify(x => x.AddAsync(custodia), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_QuandoGerarNumeroConta_DeveChamarMetodoCorretamente()
    {
        // Arrange
        var request = new AdesaoRequest
        {
            Nome = "Teste Usuario",
            Cpf = "11122233344",
            Email = "teste@teste.com",
            ValorMensal = 750
        };

        var cliente = new Cliente(request.Nome, request.Cpf, request.Email, request.ValorMensal);
        cliente.GetType().GetProperty(nameof(Cliente.Id))?.SetValue(cliente, 1L);

        var contaGrafica = new ContaGrafica(cliente.Id);
        contaGrafica.GetType().GetProperty(nameof(ContaGrafica.Id))?.SetValue(contaGrafica, 1L);

        var custodia = new Custodia(contaGrafica.Id);

        _clienteRepositoryMock.Setup(x => x.CpfExistsAsync(request.Cpf)).ReturnsAsync(false);
        _clienteFactoryMock.Setup(x => x.Criar(request.Nome, request.Cpf, request.Email, request.ValorMensal)).Returns(cliente);
        _clienteRepositoryMock.Setup(x => x.AddAsync(cliente)).ReturnsAsync(cliente);

        _contaGraficaFactoryMock.Setup(x => x.Criar(cliente.Id)).Returns(contaGrafica);
        _contaGraficaRepositoryMock.Setup(x => x.AddAsync(contaGrafica)).ReturnsAsync(contaGrafica);

        _custodiaFactoryMock.Setup(x => x.Criar(contaGrafica.Id)).Returns(custodia);
        _custodiaRepositoryMock.Setup(x => x.AddAsync(custodia)).ReturnsAsync(custodia);

        // Act
        await _realizarAdesao.ExecuteAsync(request);

        // Assert
        contaGrafica.NumeroConta.Should().NotBeNull();
        contaGrafica.NumeroConta.Should().StartWith("FLH-");
        _contaGraficaRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
    }
}
