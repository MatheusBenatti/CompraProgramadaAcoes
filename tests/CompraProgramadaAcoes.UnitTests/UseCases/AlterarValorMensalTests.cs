using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.Application.Exceptions;
using CompraProgramadaAcoes.Application.Interfaces.Repositories;
using CompraProgramadaAcoes.Application.UseCases;
using CompraProgramadaAcoes.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CompraProgramadaAcoes.UnitTests.UseCases;

public class AlterarValorMensalTests
{
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<IHistoricoValorMensalRepository> _historicoRepositoryMock;
    private readonly AlterarValorMensal _alterarValorMensal;

    public AlterarValorMensalTests()
    {
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _historicoRepositoryMock = new Mock<IHistoricoValorMensalRepository>();
        _alterarValorMensal = new AlterarValorMensal(_clienteRepositoryMock.Object, _historicoRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_QuandoClienteNaoExiste_DeveLancarClienteNaoEncontradoException()
    {
        // Arrange
        var clienteId = 999;
        var request = new AlterarValorMensalRequest { NovoValorMensal = 1000 };

        _clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(clienteId))
            .ReturnsAsync((Cliente?)null);

        // Act & Assert
        await _alterarValorMensal
            .Invoking(x => x.ExecuteAsync(clienteId, request))
            .Should()
            .ThrowAsync<ClienteNaoEncontradoException>()
            .WithMessage("Cliente não encontrado.");

        _clienteRepositoryMock.Verify(x => x.GetByIdAsync(clienteId), Times.Once);
        _historicoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<HistoricoValorMensal>()), Times.Never);
        _clienteRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_QuandoClienteEstaInativo_DeveLancarClienteJaInativoException()
    {
        // Arrange
        var clienteId = 1;
        var cliente = new Cliente("João Silva", "12345678901", "joao@teste.com", 500);
        cliente.GetType().GetProperty(nameof(Cliente.Id))?.SetValue(cliente, (long)clienteId);
        cliente.Desativar();

        var request = new AlterarValorMensalRequest { NovoValorMensal = 1000 };

        _clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(clienteId))
            .ReturnsAsync(cliente);

        // Act & Assert
        await _alterarValorMensal
            .Invoking(x => x.ExecuteAsync(clienteId, request))
            .Should()
            .ThrowAsync<ClienteJaInativoException>()
            .WithMessage("Cliente já havia saído do produto.");

        _clienteRepositoryMock.Verify(x => x.GetByIdAsync(clienteId), Times.Once);
        _historicoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<HistoricoValorMensal>()), Times.Never);
        _clienteRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_QuandoValorEhIgualAoAtual_DeveRetornarMensagemCorrespondente()
    {
        // Arrange
        var clienteId = 1;
        var cliente = new Cliente("Maria Santos", "98765432100", "maria@teste.com", 1000);
        cliente.GetType().GetProperty(nameof(Cliente.Id))?.SetValue(cliente, (long)clienteId);

        var request = new AlterarValorMensalRequest { NovoValorMensal = 1000 };

        _clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(clienteId))
            .ReturnsAsync(cliente);

        // Act
        var result = await _alterarValorMensal.ExecuteAsync(clienteId, request);

        // Assert
        result.Should().NotBeNull();
        result.ClienteId.Should().Be(clienteId);
        result.ValorMensalAnterior.Should().Be(1000);
        result.ValorMensalNovo.Should().Be(1000);
        result.Mensagem.Should().Be("O novo valor mensal é igual ao valor atual.");
        result.DataAlteracao.Should().NotBeNullOrEmpty();

        _clienteRepositoryMock.Verify(x => x.GetByIdAsync(clienteId), Times.Once);
        _historicoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<HistoricoValorMensal>()), Times.Never);
        _clienteRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_QuandoAlteracaoValida_DeveAtualizarValorComSucesso()
    {
        // Arrange
        var clienteId = 1;
        var cliente = new Cliente("Carlos Alberto", "11122233344", "carlos@teste.com", 3000);
        cliente.GetType().GetProperty(nameof(Cliente.Id))?.SetValue(cliente, (long)clienteId);

        var request = new AlterarValorMensalRequest { NovoValorMensal = 6000 };

        _clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(clienteId))
            .ReturnsAsync(cliente);

        _historicoRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<HistoricoValorMensal>()))
            .ReturnsAsync((HistoricoValorMensal h) => h);

        _clienteRepositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _alterarValorMensal.ExecuteAsync(clienteId, request);

        // Assert
        result.Should().NotBeNull();
        result.ClienteId.Should().Be(clienteId);
        result.ValorMensalAnterior.Should().Be(3000);
        result.ValorMensalNovo.Should().Be(6000);
        result.Mensagem.Should().Be("Valor mensal atualizado. O novo valor será considerado a partir da próxima data de compra.");
        result.DataAlteracao.Should().NotBeNullOrEmpty();

        // Verificar que o cliente foi atualizado
        cliente.ValorMensal.Should().Be(6000);

        // Verificar que o histórico foi criado
        _historicoRepositoryMock.Verify(x => x.AddAsync(It.Is<HistoricoValorMensal>(h => 
            h.ClienteId == clienteId && 
            h.ValorAnterior == 3000 && 
            h.ValorNovo == 6000)), Times.Once);

        _clienteRepositoryMock.Verify(x => x.GetByIdAsync(clienteId), Times.Once);
        _clienteRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_QuandoAlteracaoValida_DeveManterOutrosDadosDoCliente()
    {
        // Arrange
        var clienteId = 1;
        var nome = "Teste Usuario";
        var cpf = "55566677788";
        var email = "teste@teste.com";
        var cliente = new Cliente(nome, cpf, email, 750);
        cliente.GetType().GetProperty(nameof(Cliente.Id))?.SetValue(cliente, (long)clienteId);

        var request = new AlterarValorMensalRequest { NovoValorMensal = 1500 };

        _clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(clienteId))
            .ReturnsAsync(cliente);

        _historicoRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<HistoricoValorMensal>()))
            .ReturnsAsync((HistoricoValorMensal h) => h);

        _clienteRepositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _alterarValorMensal.ExecuteAsync(clienteId, request);

        // Assert
        result.Should().NotBeNull();
        
        // Verificar que outros dados do cliente não foram alterados
        cliente.Nome.Should().Be(nome);
        cliente.Cpf.Should().Be(cpf);
        cliente.Email.Should().Be(email);
        cliente.Ativo.Should().BeTrue();
        cliente.ValorMensal.Should().Be(1500); // Apenas o valor mensal mudou

        _clienteRepositoryMock.Verify(x => x.GetByIdAsync(clienteId), Times.Once);
        _historicoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<HistoricoValorMensal>()), Times.Once);
        _clienteRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_QuandoDataAlteracao_DeveRetornarFormatoCorreto()
    {
        // Arrange
        var clienteId = 1;
        var cliente = new Cliente("Data Teste", "99988877766", "data@teste.com", 2000);
        cliente.GetType().GetProperty(nameof(Cliente.Id))?.SetValue(cliente, (long)clienteId);

        var request = new AlterarValorMensalRequest { NovoValorMensal = 4000 };

        _clienteRepositoryMock
            .Setup(x => x.GetByIdAsync(clienteId))
            .ReturnsAsync(cliente);

        _historicoRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<HistoricoValorMensal>()))
            .ReturnsAsync((HistoricoValorMensal h) => h);

        _clienteRepositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _alterarValorMensal.ExecuteAsync(clienteId, request);

        // Assert
        result.Should().NotBeNull();
        result.DataAlteracao.Should().NotBeNullOrEmpty();
        
        // Verificar formato da data (yyyy-MM-ddTHH:mm:ssZ)
        result.DataAlteracao.Should().MatchRegex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z$");
        
        // Verificar se a data é próxima da atual (usando DateTimeOffset para UTC)
        var dataAlteracao = DateTimeOffset.Parse(result.DataAlteracao);
        dataAlteracao.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(5));

        _clienteRepositoryMock.Verify(x => x.GetByIdAsync(clienteId), Times.Once);
        _historicoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<HistoricoValorMensal>()), Times.Once);
        _clienteRepositoryMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}
