namespace CompraProgramadaAcoes.Application.Interfaces
{
  public interface ICacheService
  {
    Task SetAsync(string key, string value);
    Task<string?> GetAsync(string key);
    Task SetAsync<T>(string key, T value);
    Task<T?> GetAsync<T>(string key);
  }
}