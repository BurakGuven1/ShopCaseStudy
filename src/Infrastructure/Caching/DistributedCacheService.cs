using Application.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis; // 🔴 Infra’da serbest
using System.Text;

namespace Infrastructure.Caching;

public sealed class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<DistributedCacheService> _logger;
    private readonly IConnectionMultiplexer? _redisMux;

    public DistributedCacheService(
        IDistributedCache cache,
        ILogger<DistributedCacheService> logger,
        IServiceProvider sp)
    {
        _cache = cache;
        _logger = logger;
        _redisMux = sp.GetService<IConnectionMultiplexer>();
    }

    public async Task<string?> GetStringAsync(string key, CancellationToken ct = default)
    {
        var bytes = await _cache.GetAsync(key, ct);
        return bytes is null ? null : Encoding.UTF8.GetString(bytes);
    }

    public async Task SetStringAsync(string key, string value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        var opts = new DistributedCacheEntryOptions();
        if (expiry is not null) opts.AbsoluteExpirationRelativeToNow = expiry;
        await _cache.SetAsync(key, Encoding.UTF8.GetBytes(value), opts, ct);
    }

    public async Task<long> IncrementAsync(string key, CancellationToken ct = default)
    {
        if (_redisMux is not null && _redisMux.IsConnected)
        {
            var db = _redisMux.GetDatabase();
            return await db.StringIncrementAsync(key);
        }

        var s = await GetStringAsync(key, ct);
        if (!long.TryParse(s, out var current)) current = 0;
        var next = current + 1;
        await SetStringAsync(key, next.ToString(), null, ct);
        _logger.LogDebug("Increment fallback used for key {Key}: {Value}", key, next);
        return next;
    }
}
