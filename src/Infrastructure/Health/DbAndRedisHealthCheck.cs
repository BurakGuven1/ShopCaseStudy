using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;
using Infrastructure.Persistence;

namespace Infrastructure.Health;

public sealed class DbAndRedisHealthCheck : IHealthCheck
{
    private readonly AppDbContext _db;
    private readonly IConnectionMultiplexer? _redis;

    public DbAndRedisHealthCheck(AppDbContext db, IConnectionMultiplexer? redis = null)
    {
        _db = db;
        _redis = redis;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>();
        var unhealthy = new List<string>();
        var degraded = new List<string>();

        try
        {
            var sw = Stopwatch.StartNew();
            await _db.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
            sw.Stop();
            data["dbMs"] = sw.ElapsedMilliseconds;
            if (sw.ElapsedMilliseconds > 500) degraded.Add("db slow");
        }
        catch (Exception ex)
        {
            unhealthy.Add("db error: " + ex.Message);
        }

        if (_redis != null)
        {
            try
            {
                var sw = Stopwatch.StartNew();
                var pong = await _redis.GetDatabase().PingAsync();
                sw.Stop();
                data["redisMs"] = sw.ElapsedMilliseconds;
                if (sw.ElapsedMilliseconds > 200) degraded.Add("redis slow");
            }
            catch (Exception ex)
            {
                unhealthy.Add("redis error: " + ex.Message);
            }
        }
        else
        {
            degraded.Add("redis not configured");
        }

        if (unhealthy.Count > 0)
            return HealthCheckResult.Unhealthy(string.Join("; ", unhealthy), data: data);

        if (degraded.Count > 0)
            return HealthCheckResult.Degraded(string.Join("; ", degraded), data: data);

        return HealthCheckResult.Healthy("ok", data);
    }
}
