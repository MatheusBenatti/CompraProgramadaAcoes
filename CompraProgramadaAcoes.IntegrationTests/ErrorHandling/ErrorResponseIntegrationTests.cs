using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.IntegrationTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CompraProgramadaAcoes.IntegrationTests.ErrorHandling;

public class ErrorResponseIntegrationTests : IntegrationTestBase
{
    private readonly HttpClient _client;

    public ErrorResponseIntegrationTests()
    {
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task ErrorResponse_ComDadosInvalidos_DeveRetornarJsonEstruturaCorreta()
    {
        // Arrange
        var request = new AdesaoRequest
        {
            Nome = "",
            Cpf = "123",
            Email = "invalido",
            ValorMensal = 50
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/clientes/adesao", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        errorResponse.Should().NotBeNull();
        errorResponse!.Erro.Should().Be("Dados inválidos.");
        errorResponse.Codigo.Should().Be("REQUISICAO_INVALIDA");
    }

    [Fact]
    public async Task ErrorResponse_ComCpfDuplicado_DeveRetornarJsonEstruturaCorreta()
    {
        // Arrange
        var request = new AdesaoRequest
        {
            Nome = "CPF Duplicado Test",
            Cpf = "12345678901",
            Email = "duplicado@teste.com",
            ValorMensal = 1000
        };

        // Primeira requisição
        await _client.PostAsJsonAsync("/api/clientes/adesao", request);

        // Segunda requisição com mesmo CPF
        var response = await _client.PostAsJsonAsync("/api/clientes/adesao", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        errorResponse.Should().NotBeNull();
        errorResponse!.Erro.Should().Be("CPF ja cadastrado no sistema.");
        errorResponse.Codigo.Should().Be("CLIENTE_CPF_DUPLICADO");
    }

    [Fact]
    public async Task ErrorResponse_DeveTerContentTypeJson()
    {
        // Arrange
        var request = new AdesaoRequest
        {
            Nome = "",
            Cpf = "123",
            Email = "invalido",
            ValorMensal = 50
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/clientes/adesao", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task ErrorResponse_JsonDeveSerValido()
    {
        // Arrange
        var request = new AdesaoRequest
        {
            Nome = "",
            Cpf = "123",
            Email = "invalido",
            ValorMensal = 50
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/clientes/adesao", request);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        jsonString.Should().NotBeNullOrEmpty();
        
        // Verificar se é JSON válido
        var jsonDoc = JsonDocument.Parse(jsonString);
        jsonDoc.RootElement.TryGetProperty("erro", out var erroElement).Should().BeTrue();
        jsonDoc.RootElement.TryGetProperty("codigo", out var codigoElement).Should().BeTrue();
        
        erroElement.GetString().Should().Be("Dados inválidos.");
        codigoElement.GetString().Should().Be("REQUISICAO_INVALIDA");
    }

    [Fact]
    public async Task ErrorResponse_ComJsonInvalido_DeveRetornarErroEstruturado()
    {
        // Arrange
        var jsonInvalido = "{ \"nome\": \"Teste\", \"cpf\": invalido }";
        var content = new StringContent(jsonInvalido, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/clientes/adesao", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        
        var jsonString = await response.Content.ReadAsStringAsync();
        jsonString.Should().NotBeNullOrEmpty();
        
        // O ASP.NET Core deve retornar um erro estruturado para JSON inválido
        var jsonDoc = JsonDocument.Parse(jsonString);
        jsonDoc.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task ErrorResponse_ComNomeInvalido_DeveRetornarErroEstruturado(string nome)
    {
        // Arrange
        var request = new AdesaoRequest
        {
            Nome = nome,
            Cpf = "12345678901",
            Email = "teste@teste.com",
            ValorMensal = 1000
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/clientes/adesao", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        errorResponse.Should().NotBeNull();
        errorResponse!.Erro.Should().Be("Dados inválidos.");
        errorResponse.Codigo.Should().Be("REQUISICAO_INVALIDA");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("123456789012")]
    [InlineData("abc12345678")]
    public async Task ErrorResponse_ComCpfInvalido_DeveRetornarErroEstruturado(string cpf)
    {
        // Arrange
        var request = new AdesaoRequest
        {
            Nome = "Teste CPF",
            Cpf = cpf,
            Email = "teste@teste.com",
            ValorMensal = 1000
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/clientes/adesao", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        errorResponse.Should().NotBeNull();
        errorResponse!.Erro.Should().Be("Dados inválidos.");
        errorResponse.Codigo.Should().Be("REQUISICAO_INVALIDA");
    }

    [Theory]
    [InlineData("email-sem-arroba")]
    [InlineData("email@")]
    [InlineData("@email.com")]
    [InlineData("email.com")]
    public async Task ErrorResponse_ComEmailInvalido_DeveRetornarErroEstruturado(string email)
    {
        // Arrange
        var request = new AdesaoRequest
        {
            Nome = "Teste Email",
            Cpf = "12345678901",
            Email = email,
            ValorMensal = 1000
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/clientes/adesao", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        errorResponse.Should().NotBeNull();
        errorResponse!.Erro.Should().Be("Dados inválidos.");
        errorResponse.Codigo.Should().Be("REQUISICAO_INVALIDA");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(99.99)]
    [InlineData(-100)]
    public async Task ErrorResponse_ComValorMensalInvalido_DeveRetornarErroEstruturado(decimal valorMensal)
    {
        // Arrange
        var request = new AdesaoRequest
        {
            Nome = "Teste Valor",
            Cpf = "12345678901",
            Email = "valor@teste.com",
            ValorMensal = valorMensal
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/clientes/adesao", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        errorResponse.Should().NotBeNull();
        errorResponse!.Erro.Should().Be("Dados inválidos.");
        errorResponse.Codigo.Should().Be("REQUISICAO_INVALIDA");
    }
}
