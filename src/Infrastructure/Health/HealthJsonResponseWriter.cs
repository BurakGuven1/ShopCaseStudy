using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace Infrastructure.Health;

public static class HealthJsonResponseWriter
{
    public static Task Write(HttpContext ctx, HealthReport report)
    {
        ctx.Response.ContentType = "application/json; charset=utf-8";
        ctx.Response.StatusCode = report.Status switch
        {
            HealthStatus.Healthy => StatusCodes.Status200OK,
            HealthStatus.Degraded => StatusCodes.Status200OK,
            _ => StatusCodes.Status503ServiceUnavailable
        };

        var payload = new
        {
            status = report.Status.ToString(),
            entries = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                data = e.Value.Data
            })
        };

        return ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
