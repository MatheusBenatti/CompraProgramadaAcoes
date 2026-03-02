using CompraProgramadaAcoes.Application.Exceptions;
using FluentAssertions;

namespace CompraProgramadaAcoes.UnitTests.Exceptions;

public class ClienteJaInativoExceptionTests
{
    [Fact]
    public void Constructor_DeveCriarExcecaoComMensagemPadrao()
    {
        // Act
        var exception = new ClienteJaInativoException();

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be("Cliente já havia saído do produto.");
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void Constructor_DeveHerdarDeException()
    {
        // Act
        var exception = new ClienteJaInativoException();

        // Assert
        exception.Should().BeOfType<ClienteJaInativoException>();
        exception.Should().BeAssignableTo<Exception>();
    }
}

public class ValorMensalInvalidoExceptionTests
{
    [Fact]
    public void Constructor_DeveCriarExcecaoComMensagemPadrao()
    {
        // Act
        var exception = new ValorMensalInvalidoException();

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().Be("Valor mensal abaixo do mínimo.");
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void Constructor_DeveHerdarDeException()
    {
        // Act
        var exception = new ValorMensalInvalidoException();

        // Assert
        exception.Should().BeOfType<ValorMensalInvalidoException>();
        exception.Should().BeAssignableTo<Exception>();
    }
}
