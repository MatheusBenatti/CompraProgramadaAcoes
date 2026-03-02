using CompraProgramadaAcoes.Application.DTOs;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace CompraProgramadaAcoes.UnitTests.DTOs;

public class AlterarValorMensalRequestTests
{
    [Fact]
    public void NovoValorMensal_QuandoValido_DevePassarValidacao()
    {
        // Arrange
        var request = new AlterarValorMensalRequest { NovoValorMensal = 1000 };

        // Act
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

        // Assert
        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void NovoValorMensal_QuandoNulo_DeveFalharValidacao()
    {
        // Arrange
        var request = new AlterarValorMensalRequest { NovoValorMensal = 0 };

        // Act
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

        // Assert
        isValid.Should().BeFalse();
        validationResults.Should().Contain(v => v.ErrorMessage!.Contains("Valor mensal mínimo é R$ 100,00"));
    }

    [Fact]
    public void NovoValorMensal_QuandoAbaixoDoMinimo_DeveFalharValidacao()
    {
        // Arrange
        var request = new AlterarValorMensalRequest { NovoValorMensal = 50 };

        // Act
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

        // Assert
        isValid.Should().BeFalse();
        validationResults.Should().Contain(v => v.ErrorMessage!.Contains("Valor mensal mínimo é R$ 100,00"));
    }

    [Fact]
    public void NovoValorMensal_QuandoIgualAoMinimo_DevePassarValidacao()
    {
        // Arrange
        var request = new AlterarValorMensalRequest { NovoValorMensal = 100 };

        // Act
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

        // Assert
        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void NovoValorMensal_QuandoValorAlto_DevePassarValidacao()
    {
        // Arrange
        var request = new AlterarValorMensalRequest { NovoValorMensal = 100000 };

        // Act
        var validationContext = new ValidationContext(request);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(request, validationContext, validationResults, true);

        // Assert
        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
    }
}

public class AlterarValorMensalResponseTests
{
    [Fact]
    public void Propriedades_QuandoAtribuidas_DeveManterValores()
    {
        // Arrange
        var response = new AlterarValorMensalResponse
        {
            ClienteId = 1,
            ValorMensalAnterior = 3000,
            ValorMensalNovo = 6000,
            DataAlteracao = "2026-02-10T09:00:00Z",
            Mensagem = "Valor mensal atualizado."
        };

        // Act & Assert
        response.ClienteId.Should().Be(1);
        response.ValorMensalAnterior.Should().Be(3000);
        response.ValorMensalNovo.Should().Be(6000);
        response.DataAlteracao.Should().Be("2026-02-10T09:00:00Z");
        response.Mensagem.Should().Be("Valor mensal atualizado.");
    }

    [Fact]
    public void Propriedades_QuandoNaoAtribuidas_DeveTerValoresPadrao()
    {
        // Arrange
        var response = new AlterarValorMensalResponse();

        // Act & Assert
        response.ClienteId.Should().Be(0);
        response.ValorMensalAnterior.Should().Be(0);
        response.ValorMensalNovo.Should().Be(0);
        response.DataAlteracao.Should().BeNull();
        response.Mensagem.Should().BeNull();
    }
}
