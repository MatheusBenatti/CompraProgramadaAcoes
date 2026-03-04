using CompraProgramadaAcoes.Domain.Factories;
using FluentAssertions;

namespace CompraProgramadaAcoes.UnitTests.Factories;

public class ClienteFactoryTests
{
    private readonly ClienteFactory _factory;

    public ClienteFactoryTests()
    {
        _factory = new ClienteFactory();
    }

    [Fact]
    public void Criar_ComDadosValidos_DeveRetornarCliente()
    {
        // Arrange
        var nome = "João Silva";
        var cpf = "12345678901";
        var email = "joao@teste.com";
        var valorMensal = 500m;

        // Act
        var cliente = _factory.Criar(nome, cpf, email, valorMensal);

        // Assert
        cliente.Should().NotBeNull();
        cliente.Nome.Should().Be(nome);
        cliente.Cpf.Should().Be(cpf);
        cliente.Email.Should().Be(email);
        cliente.ValorMensal.Should().Be(valorMensal);
        cliente.Ativo.Should().BeTrue();
        cliente.DataAdesao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_ComNomeInvalido_DeveLancarArgumentException(string nome)
    {
        // Arrange
        var cpf = "12345678901";
        var email = "joao@teste.com";
        var valorMensal = 500m;

        // Act & Assert
        _factory.Invoking(f => f.Criar(nome, cpf, email, valorMensal))
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("Nome é obrigatório");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_ComCpfInvalido_DeveLancarArgumentException(string cpf)
    {
        // Arrange
        var nome = "João Silva";
        var email = "joao@teste.com";
        var valorMensal = 500m;

        // Act & Assert
        _factory.Invoking(f => f.Criar(nome, cpf, email, valorMensal))
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("CPF é obrigatório");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_ComEmailInvalido_DeveLancarArgumentException(string email)
    {
        // Arrange
        var nome = "João Silva";
        var cpf = "12345678901";
        var valorMensal = 500m;

        // Act & Assert
        _factory.Invoking(f => f.Criar(nome, cpf, email, valorMensal))
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("Email é obrigatório");
    }

    [Theory]
    [InlineData(99.99)]
    [InlineData(50)]
    [InlineData(0)]
    [InlineData(-100)]
    public void Criar_ComValorMensalInvalido_DeveLancarArgumentException(decimal valorMensal)
    {
        // Arrange
        var nome = "João Silva";
        var cpf = "12345678901";
        var email = "joao@teste.com";

        // Act & Assert
        _factory.Invoking(f => f.Criar(nome, cpf, email, valorMensal))
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("Valor mensal mínimo é R$ 100,00");
    }

    [Fact]
    public void Criar_ComValorMensalMinimo_DeveCriarCliente()
    {
        // Arrange
        var nome = "João Silva";
        var cpf = "12345678901";
        var email = "joao@teste.com";
        var valorMensal = 100m;

        // Act
        var cliente = _factory.Criar(nome, cpf, email, valorMensal);

        // Assert
        cliente.Should().NotBeNull();
        cliente.ValorMensal.Should().Be(valorMensal);
    }

    [Fact]
    public void Criar_ComValorMensalAlto_DeveCriarCliente()
    {
        // Arrange
        var nome = "João Silva";
        var cpf = "12345678901";
        var email = "joao@teste.com";
        var valorMensal = 10000m;

        // Act
        var cliente = _factory.Criar(nome, cpf, email, valorMensal);

        // Assert
        cliente.Should().NotBeNull();
        cliente.ValorMensal.Should().Be(valorMensal);
    }

    [Fact]
    public void Criar_ComNomeComEspacos_DeveCriarCliente()
    {
        // Arrange
        var nome = "  João da Silva  ";
        var cpf = "12345678901";
        var email = "joao@teste.com";
        var valorMensal = 500m;

        // Act
        var cliente = _factory.Criar(nome, cpf, email, valorMensal);

        // Assert
        cliente.Should().NotBeNull();
        cliente.Nome.Should().Be(nome);
    }
}
