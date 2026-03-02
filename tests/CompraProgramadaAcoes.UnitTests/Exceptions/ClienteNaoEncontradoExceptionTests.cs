using CompraProgramadaAcoes.Application.Exceptions;
using FluentAssertions;

namespace CompraProgramadaAcoes.UnitTests.Exceptions;

public class ClienteNaoEncontradoExceptionTests
{
    [Fact]
    public void Constructor_DeveCriarExcecaoComMensagemPadrao()
    {
        // Act
        var exception = new ClienteNaoEncontradoException();

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be("Cliente não encontrado.");
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void Constructor_DeveHerdarDeException()
    {
        // Act
        var exception = new ClienteNaoEncontradoException();

        // Assert
        exception.Should().BeOfType<ClienteNaoEncontradoException>();
        exception.Should().BeAssignableTo<Exception>();
    }
}
