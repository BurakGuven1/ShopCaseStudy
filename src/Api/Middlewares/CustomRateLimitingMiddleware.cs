using System.Collections.Concurrent;

namespace Api.Middlewares
{
    public class CustomRateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ConcurrentDictionary<string, List<DateTime>> _requestTimestamps = new();
        private readonly ILogger<CustomRateLimitingMiddleware> _logger;

        public CustomRateLimitingMiddleware(RequestDelegate next, ILogger<CustomRateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var now = DateTime.UtcNow;
            var limit = 5; 
            var window = TimeSpan.FromSeconds(10);

            _logger.LogInformation($"Request from IP: {ip}, Time: {now}");

            var timestamps = _requestTimestamps.GetOrAdd(ip, new List<DateTime>());

            timestamps.RemoveAll(t => now - t > window);

            _logger.LogInformation($"Current request count for {ip}: {timestamps.Count}");

            if (timestamps.Count >= limit)
            {
                _logger.LogWarning($"Rate limit exceeded for IP: {ip}");
                context.Response.StatusCode = 429;
                await context.Response.WriteAsync("Rate limit exceeded. Try again later.");
                return;
            }

            timestamps.Add(now);
            _requestTimestamps[ip] = timestamps;

            _logger.LogInformation($"Request allowed for IP: {ip}, new count: {timestamps.Count}");

            await _next(context);
        }
    }
}