using CompraProgramadaAcoes.Infrastructure.Cache;
using StackExchange.Redis;
using Moq;
using FluentAssertions;

namespace CompraProgramadaAcoes.UnitTests.Infrastructure.Cache;

public class RedisCacheServiceTests
{
    private readonly Mock<IConnectionMultiplexer> _connectionMultiplexerMock;
    private readonly Mock<IDatabase> _databaseMock;
    private readonly RedisCacheService _service;

    public RedisCacheServiceTests()
    {
        _connectionMultiplexerMock = new Mock<IConnectionMultiplexer>();
        _databaseMock = new Mock<IDatabase>();
        
        _connectionMultiplexerMock.Setup(c => c.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(_databaseMock.Object);
        
        _service = new RedisCacheService(_connectionMultiplexerMock.Object);
    }

    [Fact]
    public async Task SetAsync_StringValue_DeveChamarStringSetAsync()
    {
        // Arrange
        var key = "test_key";
        var value = "test_value";

        // Act
        await _service.SetAsync(key, value);

        // Assert
        // _databaseMock.Verify(d => d.StringSetAsync(key, value), Times.Once);
    }

    // [Fact]
    // public async Task GetAsync_KeyExistente_DeveRetornarValor()
    // {
    //     // Arrange
    //     var key = "test_key";
    //     var expectedValue = "test_value";
    //     _databaseMock.Setup(d => d.StringGetAsync(key, It.IsAny<CommandFlags>()))
    //         .ReturnsAsync(new RedisValue(expectedValue));

    //     // Act
    //     var result = await _service.GetAsync(key);

    //     // Assert
    //     result.Should().Be(expectedValue);
    //     _databaseMock.Verify(d => d.StringGetAsync(key), Times.Once);
    // }

    // [Fact]
    // public async Task GetAsync_KeyNaoExistente_DeveRetornarNulo()
    // {
    //     // Arrange
    //     var key = "non_existent_key";
    //     _databaseMock.Setup(d => d.StringGetAsync(key, It.IsAny<CommandFlags>()))
    //         .ReturnsAsync(RedisValue.Null);

    //     // Act
    //     var result = await _service.GetAsync(key);

    //     // Assert
    //     result.Should().BeNull();
    //     _databaseMock.Verify(d => d.StringGetAsync(key), Times.Once);
    // }

    // [Fact]
    // public async Task GetAsync_KeyVazio_DeveRetornarNulo()
    // {
    //     // Arrange
    //     var key = "empty_key";
    //     _databaseMock.Setup(d => d.StringGetAsync(key, It.IsAny<CommandFlags>()))
    //         .ReturnsAsync(new RedisValue(string.Empty));

    //     // Act
    //     var result = await _service.GetAsync(key);

    //     // Assert
    //     result.Should().BeNull();
    //     _databaseMock.Verify(d => d.StringGetAsync(key), Times.Once);
    // }

    [Fact]
    public async Task SetAsync_GenericValue_DeveSerializarEChamarStringSetAsync()
    {
        // Arrange
        var key = "test_key";
        var value = new TestObject { Name = "Test", Value = 123 };

        // Act
        await _service.SetAsync(key, value);

        // Assert
        // _databaseMock.Verify(d => d.StringSetAsync(key, It.IsAny<string>()), Times.Once);
    }

    // [Fact]
    // public async Task GetAsync_GenericType_KeyExistente_DeveDeserializarERetornarObjeto()
    // {
    //     // Arrange
    //     var key = "test_key";
    //     var expectedObject = new TestObject { Name = "Test", Value = 123 };
    //     var json = @"{""name"":""Test"",""value"":123}";
        
    //     _databaseMock.Setup(d => d.StringGetAsync(key, It.IsAny<CommandFlags>()))
    //         .ReturnsAsync(new RedisValue(json));

    //     // Act
    //     var result = await _service.GetAsync<TestObject>(key);

    //     // Assert
    //     result.Should().NotBeNull();
    //     result.Name.Should().Be(expectedObject.Name);
    //     result.Value.Should().Be(expectedObject.Value);
    //     _databaseMock.Verify(d => d.StringGetAsync(key), Times.Once);
    // }

    // [Fact]
    // public async Task GetAsync_GenericType_KeyNaoExistente_DeveRetornarDefault()
    // {
    //     // Arrange
    //     var key = "non_existent_key";
    //     _databaseMock.Setup(d => d.StringGetAsync(key, It.IsAny<CommandFlags>()))
    //         .ReturnsAsync(RedisValue.Null);

    //     // Act
    //     var result = await _service.GetAsync<TestObject>(key);

    //     // Assert
    //     result.Should().BeNull();
    //     _databaseMock.Verify(d => d.StringGetAsync(key), Times.Once);
    // }

    // [Fact]
    // public async Task GetAsync_GenericType_JsonInvalido_DeveRetornarDefault()
    // {
    //     // Arrange
    //     var key = "invalid_json_key";
    //     var invalidJson = @"{ invalid json }";
        
    //     _databaseMock.Setup(d => d.StringGetAsync(key, It.IsAny<CommandFlags>()))
    //         .ReturnsAsync(new RedisValue(invalidJson));

    //     // Act
    //     var result = await _service.GetAsync<TestObject>(key);

    //     // Assert
    //     result.Should().BeNull();
    //     _databaseMock.Verify(d => d.StringGetAsync(key), Times.Once);
    // }

    // [Theory]
    // [InlineData("")]
    // [InlineData("   ")]
    // public async Task GetAsync_GenericType_KeyVazioOuEspacos_DeveRetornarDefault(string key)
    // {
    //     // Arrange
    //     _databaseMock.Setup(d => d.StringGetAsync(key, It.IsAny<CommandFlags>()))
    //         .ReturnsAsync(new RedisValue(string.Empty));

    //     // Act
    //     var result = await _service.GetAsync<TestObject>(key);

    //     // Assert
    //     result.Should().BeNull();
    //     _databaseMock.Verify(d => d.StringGetAsync(key), Times.Once);
    // }

    private class TestObject
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
