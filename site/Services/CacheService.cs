using System.Text.Json;
using StackExchange.Redis;

namespace MarketingSite.Services;

public interface ICacheService : IDisposable
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;
    Task PingAsync(CancellationToken cancellationToken = default);
}

public class CacheService(IConfiguration configuration) : ICacheService
{
    private readonly Lazy<ConnectionMultiplexer> _lazyConnection = new(() =>
    {
        var options = ConfigurationOptions.Parse(configuration["REDIS_CONNECTION_STRING"] ??
                                                 throw new InvalidOperationException(
                                                     "REDIS_CONNECTION_STRING is not configured."));
        options.AbortOnConnectFail = false;
        return ConnectionMultiplexer.Connect(options);
    });

    private IDatabase Database => _lazyConnection.Value.GetDatabase();

    public void Dispose()
    {
        if (_lazyConnection.IsValueCreated)
            _lazyConnection.Value.Dispose();
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var value = await Database.StringGetAsync(key);
        return value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        var json = JsonSerializer.Serialize(value);
        await Database.StringSetAsync(key, json, expiry);
    }

    public async Task PingAsync(CancellationToken cancellationToken = default)
    {
        await Database.PingAsync();
    }
}