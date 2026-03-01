using System.Net;
using System.Net.Http.Json;
using System.Text;
using CompraProgramadaAcoes.Application.DTOs;
using CompraProgramadaAcoes.IntegrationTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CompraProgramadaAcoes.IntegrationTests.Controllers;

public class ClientesControllerIntegrationTests : IntegrationTestBase
{
    private readonly HttpClient _client;

    public ClientesControllerIntegrationTests()
    {
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Adesao_ComRequestValido_DeveRetornar201Created()
    {
        // Arrange
        var request = new AdesaoRequest
        {
            Nome = "João Silva",
            Cpf = "12345678901",
            Email = "joao@teste.com",
            ValorMensal = 1000
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/clientes/adesao", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadFromJsonAsync<AdesaoResponse>();
        content.Should().NotBeNull();
        content!.Nome.Should().Be(request.Nome);
        content.Cpf.Should().Be(request.Cpf);
        content.Email.Should().Be(request.Email);
        content.ValorMensal.Should().Be(request.ValorMensal);
        content.Ativo.Should().BeTrue();
        content.ContaGrafica.Should().NotBeNull();
        content.ContaGrafica.Tipo.Should().Be("FILHOTE");
        content.ContaGrafica.NumeroConta.Should().StartWith("FLH-");
    }

    [Fact]
    public async Task Adesao_ComNomeInvalido_DeveRetornar400BadRequest()
    {
        // Arrange
        var request = new AdesaoRequest
        {
            Nome = "",
            Cpf = "12345678901",
            Email = "joao@teste.com",
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

    [Fact]
    public async Task Adesao_ComCpfInvalido_DeveRetornar400BadRequest()
    {
        // Arrange
        var request = new AdesaoRequest
        {
            Nome = "João Silva",
            Cpf = "123", // CPF inválido
            Email = "joao@teste.com",
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

    [Fact]
    public async Task Adesao_ComEmailInvalido_DeveRetornar400BadRequest()
    {
        // Arrange
        var request = new AdesaoRequest
        {
            Nome = "João Silva",
            Cpf = "12345678901",
            Email = "email-invalido",
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

    [Fact]
    public async Task Adesao_ComValorMensalInvalido_DeveRetornar400BadRequest()
    {
        // Arrange
        var request = new AdesaoRequest
        {
            Nome = "João Silva",
            Cpf = "12345678901",
            Email = "joao@teste.com",
            ValorMensal = 50 // Valor abaixo do mínimo
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
    public async Task Adesao_ComCpfDuplicado_DeveRetornar400BadRequest()
    {
        // Arrange
        var request = new AdesaoRequest
        {
            Nome = "Maria Santos",
            Cpf = "98765432100",
            Email = "maria@teste.com",
            ValorMensal = 500
        };

        // Primeira requisição - deve criar o cliente
        var firstResponse = await _client.PostAsJsonAsync("/api/clientes/adesao", request);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Segunda requisição com mesmo CPF - deve retornar erro
        var secondResponse = await _client.PostAsJsonAsync("/api/clientes/adesao", request);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorResponse = await secondResponse.Content.ReadFromJsonAsync<ErrorResponse>();
        errorResponse.Should().NotBeNull();
        errorResponse!.Erro.Should().Be("CPF ja cadastrado no sistema.");
        errorResponse.Codigo.Should().Be("CLIENTE_CPF_DUPLICADO");
    }

    [Fact]
    public async Task Adesao_ComRequestNulo_DeveRetornar400BadRequest()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/clientes/adesao", (AdesaoRequest?)null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Adesao_ComJsonInvalido_DeveRetornar400BadRequest()
    {
        // Arrange
        var jsonInvalido = "{ \"nome\": \"Teste\", \"cpf\": invalido }";
        var content = new StringContent(jsonInvalido, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/clientes/adesao", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Adesao_SerializacaoJson_DeveFuncionarCorretamente()
    {
        // Arrange
        var request = new AdesaoRequest
        {
            Nome = "Teste Serialização",
            Cpf = "11122233344",
            Email = "teste@serializacao.com",
            ValorMensal = 750.50m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/clientes/adesao", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("\"nome\":\"Teste Serialização\"");
        content.Should().Contain("\"cpf\":\"11122233344\"");
        content.Should().Contain("\"valorMensal\":750.5");
    }

    [Fact]
    public async Task Adesao_HeaderLocation_DeveEstarPresente()
    {
        // Arrange
        var request = new AdesaoRequest
        {
            Nome = "Teste Location",
            Cpf = "55566677788",
            Email = "location@teste.com",
            ValorMensal = 200
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/clientes/adesao", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/api/clientes/adesao");
    }

    [Fact]
    public async Task Adesao_ComMetodoInvalido_DeveRetornar405MethodNotAllowed()
    {
        // Act
        var response = await _client.GetAsync("/api/clientes/adesao");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task Adesao_ComEndpointInvalido_DeveRetornar404NotFound()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/clientes/endpoint-inexistente", new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
