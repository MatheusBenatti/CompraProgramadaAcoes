using CompraProgramadaAcoes.Domain.Entities;
using FluentAssertions;

namespace CompraProgramadaAcoes.UnitTests.Domain.Entities;

public class ClienteTests
{
    [Fact]
    public void Cliente_Constructor_DeveInicializarCorretamente()
    {
        // Arrange
        var nome = "João Silva";
        var cpf = "12345678901";
        var email = "joao@teste.com";
        var valorMensal = 500m;

        // Act
        var cliente = new Cliente(nome, cpf, email, valorMensal);

        // Assert
        cliente.Nome.Should().Be(nome);
        cliente.Cpf.Should().Be(cpf);
        cliente.Email.Should().Be(email);
        cliente.ValorMensal.Should().Be(valorMensal);
        cliente.Ativo.Should().BeTrue();
        cliente.DataAdesao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        cliente.Id.Should().Be(0); // ID padrão quando não persistido
    }

    [Fact]
    public void AssociarContaGrafica_DeveAssociarContaCorretamente()
    {
        // Arrange
        var cliente = new Cliente("Teste", "12345678901", "teste@teste.com", 100m);
        var contaGrafica = new ContaGrafica(1);

        // Act
        cliente.AssociarContaGrafica(contaGrafica);

        // Assert
        cliente.ContaGrafica.Should().Be(contaGrafica);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(99.99)]
    public void Cliente_Constructor_ComValorMensalInvalido_DevePermitirCriacao(decimal valorMensal)
    {
        // Arrange & Act
        var cliente = new Cliente("Teste", "12345678901", "teste@teste.com", valorMensal);

        // Assert
        cliente.ValorMensal.Should().Be(valorMensal);
    }
}

public class ContaGraficaTests
{
    [Fact]
    public void ContaGrafica_Constructor_DeveInicializarCorretamente()
    {
        // Arrange
        var clienteId = 1L;

        // Act
        var contaGrafica = new ContaGrafica(clienteId);

        // Assert
        contaGrafica.ClienteId.Should().Be(clienteId);
        contaGrafica.Tipo.Should().Be("FILHOTE");
        contaGrafica.NumeroConta.Should().BeNull();
        contaGrafica.DataCriacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        contaGrafica.Id.Should().Be(0); // ID padrão quando não persistido
        contaGrafica.Custodias.Should().BeEmpty();
    }

    [Fact]
    public void GerarNumeroConta_ComIdValido_DeveGerarNumeroFormatado()
    {
        // Arrange
        var contaGrafica = new ContaGrafica(1L);
        contaGrafica.GetType().GetProperty(nameof(ContaGrafica.Id))?.SetValue(contaGrafica, 123L);

        // Act
        contaGrafica.GerarNumeroConta();

        // Assert
        contaGrafica.NumeroConta.Should().NotBeNull();
        contaGrafica.NumeroConta.Should().StartWith("FLH-");
        contaGrafica.NumeroConta.Should().EndWith("000123");
    }

    [Fact]
    public void GerarNumeroConta_ComIdInvalido_DeveLancarInvalidOperationException()
    {
        // Arrange
        var contaGrafica = new ContaGrafica(1L);
        // ID permanece 0 (inválido)

        // Act & Assert
        contaGrafica
            .Invoking(x => x.GerarNumeroConta())
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage("ID não disponível para gerar número da conta");
    }

    [Fact]
    public void GerarNumeroConta_DeveGerarNumerosUnicos()
    {
        // Arrange
        var conta1 = new ContaGrafica(1L);
        var conta2 = new ContaGrafica(2L);
        
        conta1.GetType().GetProperty(nameof(ContaGrafica.Id))?.SetValue(conta1, 100L);
        conta2.GetType().GetProperty(nameof(ContaGrafica.Id))?.SetValue(conta2, 200L);

        // Act
        conta1.GerarNumeroConta();
        conta2.GerarNumeroConta();

        // Assert
        conta1.NumeroConta.Should().NotBe(conta2.NumeroConta);
    }
}

public class CustodiaTests
{
    [Fact]
    public void Custodia_Constructor_DeveInicializarCorretamente()
    {
        // Arrange
        var contaGraficaId = 1L;

        // Act
        var custodia = new Custodia(contaGraficaId);

        // Assert
        custodia.ContaGraficaId.Should().Be(contaGraficaId);
        custodia.Id.Should().Be(0); // ID padrão quando não persistido
    }
}
