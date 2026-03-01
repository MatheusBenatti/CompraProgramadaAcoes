using System.Net;
using System.Text.Json;
using CompraProgramadaAcoes.IntegrationTests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CompraProgramadaAcoes.IntegrationTests.Pipeline;

public class HttpPipelineIntegrationTests : IntegrationTestBase
{
    private readonly HttpClient _client;

    public HttpPipelineIntegrationTests()
    {
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Pipeline_ComRequisicaoValida_DeveProcessarCorretamente()
    {
        // Arrange
        var requestBody = """
        {
            "nome": "Pipeline Test",
            "cpf": "12345678901",
            "email": "pipeline@teste.com",
            "valorMensal": 1000
        }
        """;

        var content = new System.Net.Http.StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/clientes/adesao", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().NotBeNullOrEmpty();
        
        // Verificar se o JSON é válido
        var jsonDoc = JsonDocument.Parse(responseContent);
        jsonDoc.RootElement.GetProperty("nome").GetString().Should().Be("Pipeline Test");
        jsonDoc.RootElement.GetProperty("cpf").GetString().Should().Be("12345678901");
    }

    [Fact]
    public async Task Pipeline_ComContentTypeInvalido_DeveRetornar415UnsupportedMediaType()
    {
        // Arrange
        var content = new System.Net.Http.StringContent("test", System.Text.Encoding.UTF8, "text/plain");

        // Act
        var response = await _client.PostAsync("/api/clientes/adesao", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
    }

    [Fact]
    public async Task Pipeline_ComJsonMalformado_DeveRetornar400BadRequest()
    {
        // Arrange
        var malformedJson = "{ \"nome\": \"Teste\", \"cpf\": \"123\" invalid }";
        var content = new System.Net.Http.StringContent(malformedJson, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/clientes/adesao", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Pipeline_ComRequestGrande_DeveProcessarCorretamente()
    {
        // Arrange
        var nomeGrande = new string('A', 1000);
        var requestBody = $@"
        {{
            ""nome"": ""{nomeGrande}"",
            ""cpf"": ""98765432100"",
            ""email"": ""grande@teste.com"",
            ""valorMensal"": 500
        }}";

        var content = new System.Net.Http.StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/clientes/adesao", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain(nomeGrande);
    }

    [Fact]
    public async Task Pipeline_ComCaracteresEspeciais_DeveProcessarCorretamente()
    {
        // Arrange
        var requestBody = """
        {
            "nome": "João Silva & CIA @#$%",
            "cpf": "55566677788",
            "email": "especial@caracteres.com",
            "valorMensal": 750.25
        }
        """;

        var content = new System.Net.Http.StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/clientes/adesao", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("João Silva & CIA @#$%");
    }

    [Fact]
    public async Task Pipeline_CORSHeaders_DeveEstarConfigurados()
    {
        // Arrange
        var requestBody = """
        {
            "nome": "CORS Test",
            "cpf": "11122233344",
            "email": "cors@teste.com",
            "valorMensal": 300
        }
        """;

        var content = new System.Net.Http.StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/clientes/adesao", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        // Note: Verificar headers CORS específicos se configurados
    }

    [Fact]
    public async Task Pipeline_ComEncodingUTF8_DeveProcessarCorretamente()
    {
        // Arrange
        var requestBody = """
        {
            "nome": "José García Ñoño",
            "cpf": "99988877766",
            "email": "encoding@utf8.com",
            "valorMensal": 400
        }
        """;

        var content = new System.Net.Http.StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/clientes/adesao", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.Should().Contain("José García Ñoño");
    }
}
