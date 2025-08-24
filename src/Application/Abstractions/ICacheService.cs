namespace Application.Abstractions;
public interface ICacheService
{
    Task<string?> GetStringAsync(string key, CancellationToken ct = default);
    Task SetStringAsync(string key, string value, TimeSpan? expiry = null, CancellationToken ct = default);
    Task<long> IncrementAsync(string key, CancellationToken ct = default);
}

