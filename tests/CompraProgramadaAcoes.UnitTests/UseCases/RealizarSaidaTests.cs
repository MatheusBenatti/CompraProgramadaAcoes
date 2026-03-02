using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Application.Exceptions;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Application.UseCases;
using CompraProgramadaAcoes.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CompraProgramadaAcoes.UnitTests.UseCases;

public class RealizarSaidaTests
{
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly RealizarSaida _realizarSaida;

    public RealizarSaidaTests()
    {
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _realizarSaida = new RealizarSaida(_clienteRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_QuandoClienteNaoExiste_DeveLancarClienteNaoEncontradoException()
    {
        // Arrange
        var clienteId = 999;

        _clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(clienteId))
            .ReturnsAsync((Cliente?)null);

        // Act & Assert
        await _realizarSaida
            .Invoking(x => x.ExecuteAsync(clienteId))
            .Should()
            .ThrowAsync<ClienteNaoEncontradoException>()
            .WithMessage("Cliente não encontrado.");

        _clienteRepositoryMock.Verify(x => x.GetByIdAsync(clienteId), Times.Once);
        _clienteRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_QuandoClienteJaEstaInativo_DeveRetornarMensagemCorrespondente()
    {
        // Arrange
        var clienteId = 1;
        var cliente = new Cliente("João Silva", "12345678901", "joao@teste.com", 500);
        cliente.GetType().GetProperty(nameof(Cliente.Id))?.SetValue(cliente, (long)clienteId);
        
        // Desativar o cliente manualmente para simular que já está inativo
        cliente.Desativar();

        _clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(clienteId))
            .ReturnsAsync(cliente);

        // Act
        var result = await _realizarSaida.ExecuteAsync(clienteId);

        // Assert
        result.Should().NotBeNull();
        result.ClienteId.Should().Be(clienteId);
        result.Nome.Should().Be(cliente.Nome);
        result.Ativo.Should().BeFalse();
        result.Mensagem.Should().Be("Cliente já está inativo.");
        result.DataSaida.Should().NotBeNullOrEmpty();

        _clienteRepositoryMock.Verify(x => x.GetByIdAsync(clienteId), Times.Once);
        _clienteRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_QuandoClienteEstaAtivo_DeveDesativarComSucesso()
    {
        // Arrange
        var clienteId = 1;
        var cliente = new Cliente("Maria Santos", "98765432100", "maria@teste.com", 1000);
        cliente.GetType().GetProperty(nameof(Cliente.Id))?.SetValue(cliente, (long)clienteId);

        _clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(clienteId))
            .ReturnsAsync(cliente);

        _clienteRepositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _realizarSaida.ExecuteAsync(clienteId);

        // Assert
        result.Should().NotBeNull();
        result.ClienteId.Should().Be(clienteId);
        result.Nome.Should().Be(cliente.Nome);
        result.Ativo.Should().BeFalse();
        result.Mensagem.Should().Be("Adesão encerrada. Sua posição em custódia foi mantida.");
        result.DataSaida.Should().NotBeNullOrEmpty();

        // Verificar que o cliente foi desativado
        cliente.Ativo.Should().BeFalse();

        _clienteRepositoryMock.Verify(x => x.GetByIdAsync(clienteId), Times.Once);
        _clienteRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_QuandoClienteEstaAtivo_DeveManterDadosDoCliente()
    {
        // Arrange
        var clienteId = 1;
        var nome = "Carlos Alberto";
        var cliente = new Cliente(nome, "11122233344", "carlos@teste.com", 750);
        cliente.GetType().GetProperty(nameof(Cliente.Id))?.SetValue(cliente, (long)clienteId);

        _clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(clienteId))
            .ReturnsAsync(cliente);

        _clienteRepositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _realizarSaida.ExecuteAsync(clienteId);

        // Assert
        result.Should().NotBeNull();
        result.ClienteId.Should().Be(clienteId);
        result.Nome.Should().Be(nome);
        result.Ativo.Should().BeFalse();
        
        // Verificar que outros dados do cliente não foram alterados
        cliente.Nome.Should().Be(nome);
        cliente.Cpf.Should().Be("11122233344");
        cliente.Email.Should().Be("carlos@teste.com");
        cliente.ValorMensal.Should().Be(750);

        _clienteRepositoryMock.Verify(x => x.GetByIdAsync(clienteId), Times.Once);
        _clienteRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_QuandoDataSaida_DeveRetornarFormatoCorreto()
    {
        // Arrange
        var clienteId = 1;
        var cliente = new Cliente("Teste Usuario", "55566677788", "teste@teste.com", 300);
        cliente.GetType().GetProperty(nameof(Cliente.Id))?.SetValue(cliente, (long)clienteId);

        _clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(clienteId))
            .ReturnsAsync(cliente);

        _clienteRepositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _realizarSaida.ExecuteAsync(clienteId);

        // Assert
        result.Should().NotBeNull();
        result.DataSaida.Should().NotBeNullOrEmpty();
        
        // Verificar formato da data (yyyy-MM-ddTHH:mm)
        var dataSaida = DateTime.Parse(result.DataSaida);
        dataSaida.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        _clienteRepositoryMock.Verify(x => x.GetByIdAsync(clienteId), Times.Once);
        _clienteRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
