using CompraProgramadaAcoes.Application.Mappers;
using CompraProgramadaAcoes.Domain.Entities;
using FluentAssertions;

namespace CompraProgramadaAcoes.UnitTests.Mappers;

public class AdesaoResponseMapTests
{
    [Fact]
    public void MapearParaResponse_ComDadosValidos_DeveMapearCorretamente()
    {
        // Arrange
        var cliente = new Cliente("João Silva", "12345678901", "joao@teste.com", 1000m);
        cliente.GetType().GetProperty(nameof(Cliente.Id))?.SetValue(cliente, 1L);

        var contaGrafica = new ContaGrafica(cliente.Id);
        contaGrafica.GetType().GetProperty(nameof(ContaGrafica.Id))?.SetValue(contaGrafica, 1L);
        contaGrafica.GerarNumeroConta();

        // Act
        var result = AdesaoResponseMap.MapearParaResponse(cliente, contaGrafica);

        // Assert
        result.Should().NotBeNull();
        result.ClienteId.Should().Be(cliente.Id);
        result.Nome.Should().Be(cliente.Nome);
        result.Cpf.Should().Be(cliente.Cpf);
        result.Email.Should().Be(cliente.Email);
        result.ValorMensal.Should().Be(cliente.ValorMensal);
        result.Ativo.Should().Be(cliente.Ativo);
        result.DataAdesao.Should().Be(cliente.DataAdesao.ToString("yyyy-MM-ddTHH:mm:ss"));

        result.ContaGrafica.Should().NotBeNull();
        result.ContaGrafica.Id.Should().Be(contaGrafica.Id);
        result.ContaGrafica.NumeroConta.Should().Be(contaGrafica.NumeroConta);
        result.ContaGrafica.Tipo.Should().Be(contaGrafica.Tipo);
        result.ContaGrafica.DataCriacao.Should().Be(contaGrafica.DataCriacao.ToString("yyyy-MM-ddTHH:mm:ss"));
    }

    [Fact]
    public void MapearParaResponse_ComClienteInativo_DeveMapearCorretamente()
    {
        // Arrange
        var cliente = new Cliente("Maria Santos", "98765432100", "maria@teste.com", 500m);
        cliente.GetType().GetProperty(nameof(Cliente.Id))?.SetValue(cliente, 2L);
        cliente.GetType().GetProperty(nameof(Cliente.Ativo))?.SetValue(cliente, false);

        var contaGrafica = new ContaGrafica(cliente.Id);
        contaGrafica.GetType().GetProperty(nameof(ContaGrafica.Id))?.SetValue(contaGrafica, 2L);
        contaGrafica.GerarNumeroConta();

        // Act
        var result = AdesaoResponseMap.MapearParaResponse(cliente, contaGrafica);

        // Assert
        result.Ativo.Should().BeFalse();
        result.Nome.Should().Be(cliente.Nome);
    }

    [Fact]
    public void MapearParaResponse_ComValoresDecimais_DeveMapearCorretamente()
    {
        // Arrange
        var valorMensal = 1234.56m;
        var cliente = new Cliente("Teste Usuario", "11122233344", "teste@teste.com", valorMensal);
        cliente.GetType().GetProperty(nameof(Cliente.Id))?.SetValue(cliente, 3L);

        var contaGrafica = new ContaGrafica(cliente.Id);
        contaGrafica.GetType().GetProperty(nameof(ContaGrafica.Id))?.SetValue(contaGrafica, 3L);
        contaGrafica.GerarNumeroConta();

        // Act
        var result = AdesaoResponseMap.MapearParaResponse(cliente, contaGrafica);

        // Assert
        result.ValorMensal.Should().Be(valorMensal);
    }

    [Fact]
    public void MapearParaResponse_FormatoData_DeveSerPadraoIso()
    {
        // Arrange
        var cliente = new Cliente("Teste Data", "55566677788", "data@teste.com", 100m);
        cliente.GetType().GetProperty(nameof(Cliente.Id))?.SetValue(cliente, 4L);

        var contaGrafica = new ContaGrafica(cliente.Id);
        contaGrafica.GetType().GetProperty(nameof(ContaGrafica.Id))?.SetValue(contaGrafica, 4L);
        contaGrafica.GerarNumeroConta();

        // Act
        var result = AdesaoResponseMap.MapearParaResponse(cliente, contaGrafica);

        // Assert
        result.DataAdesao.Should().MatchRegex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}$");
        result.ContaGrafica.DataCriacao.Should().MatchRegex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}$");
    }

    [Fact]
    public void MapearParaResponse_TipoContaGrafica_DeveSerFilhote()
    {
        // Arrange
        var cliente = new Cliente("Teste Tipo", "99988877766", "tipo@teste.com", 200m);
        cliente.GetType().GetProperty(nameof(Cliente.Id))?.SetValue(cliente, 5L);

        var contaGrafica = new ContaGrafica(cliente.Id);
        contaGrafica.GetType().GetProperty(nameof(ContaGrafica.Id))?.SetValue(contaGrafica, 5L);
        contaGrafica.GerarNumeroConta();

        // Act
        var result = AdesaoResponseMap.MapearParaResponse(cliente, contaGrafica);

        // Assert
        result.ContaGrafica.Tipo.Should().Be("FILHOTE");
    }

    [Fact]
    public void MapearParaResponse_NumeroContaFormato_DeveSerCorreto()
    {
        // Arrange
        var cliente = new Cliente("Teste Numero", "77766655544", "numero@teste.com", 300m);
        cliente.GetType().GetProperty(nameof(Cliente.Id))?.SetValue(cliente, 6L);

        var contaGrafica = new ContaGrafica(cliente.Id);
        contaGrafica.GetType().GetProperty(nameof(ContaGrafica.Id))?.SetValue(contaGrafica, 12345L);
        contaGrafica.GerarNumeroConta();

        // Act
        var result = AdesaoResponseMap.MapearParaResponse(cliente, contaGrafica);

        // Assert
        result.ContaGrafica.NumeroConta.Should().StartWith("FLH-");
        result.ContaGrafica.NumeroConta.Should().EndWith("012345");
    }
}
