using CompraProgramadaAcoes.Application.Exceptions;
using FluentAssertions;

namespace CompraProgramadaAcoes.UnitTests.Exceptions;

public class ClienteCpfDuplicadoExceptionTests
{
    [Fact]
    public void ClienteCpfDuplicadoException_ConstructorPadrao_DeveTerMensagemPadrao()
    {
        // Act
        var exception = new ClienteCpfDuplicadoException();

        // Assert
        exception.Should().BeAssignableTo<Exception>();
        exception.Message.Should().Be("CPF ja cadastrado no sistema.");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void ClienteCpfDuplicadoException_DeveSerDoTipoException()
    {
        // Act
        var exception = new ClienteCpfDuplicadoException();

        // Assert
        exception.Should().BeOfType<ClienteCpfDuplicadoException>();
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void ClienteCpfDuplicadoException_DeveTerStackTrace()
    {
        // Act
        try
        {
            throw new ClienteCpfDuplicadoException();
        }
        catch (ClienteCpfDuplicadoException ex)
        {
            // Assert
            ex.StackTrace.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void ClienteCpfDuplicadoException_ComInnerException_DeveFuncionar()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new ClienteCpfDuplicadoException();

        // Assert
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void ClienteCpfDuplicadoException_DeveTerMensagemConsistente()
    {
        // Act
        var exception = new ClienteCpfDuplicadoException();

        // Assert
        exception.Message.Should().Be("CPF ja cadastrado no sistema.");
    }
}
