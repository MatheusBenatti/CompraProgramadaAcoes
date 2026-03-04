using CompraProgramadaAcoes.Application.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace CompraProgramadaAcoes.Infrastructure.Cache;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task SetAsync(string key, string value)
    {
        var db = _connectionMultiplexer.GetDatabase();
        await db.StringSetAsync(key, value);
    }

    public async Task<string?> GetAsync(string key)
    {
        var db = _connectionMultiplexer.GetDatabase();
        var value = await db.StringGetAsync(key);
        return value.IsNullOrEmpty ? null : value.ToString();
    }

    public async Task SetAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value, _jsonOptions);
        await SetAsync(key, json);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var json = await GetAsync(key);
        return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }
}
