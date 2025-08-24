using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Api.Middlewares;

public class SharedResource { }

public class ProblemDetailsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IStringLocalizer<SharedResource> _loc;
    private readonly ILogger<ProblemDetailsMiddleware> _logger;

    public ProblemDetailsMiddleware(
        RequestDelegate next,
        IStringLocalizer<SharedResource> loc,
        ILogger<ProblemDetailsMiddleware> logger)
    {
        _next = next;
        _loc = loc;
        _logger = logger;
    }

    public async Task Invoke(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (KeyNotFoundException ex)
        {
            ctx.Response.Clear();
            _logger.LogWarning(ex, "Resource not found: {Message}", ex.Message);
            await Write(ctx, (int)HttpStatusCode.NotFound, _loc["Error_NotFound"], ex.Message);
            return;
        }
        catch (UnauthorizedAccessException ex)
        {
            ctx.Response.Clear();
            _logger.LogWarning(ex, "Unauthorized access: {Message}", ex.Message);
            await Write(ctx, (int)HttpStatusCode.Unauthorized, _loc["Error_Unauthorized"], ex.Message);
            return;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation exception occurred.");
            await Write(ctx, (int)HttpStatusCode.Conflict, _loc["Error_Conflict"], ex.Message);
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation exception occurred.");
            var details = string.Join("; ", ex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
            await Write(ctx, 400, _loc["Error_Validation"], details);
        }
        catch (Exception ex)
        {
            ctx.Response.Clear();
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await Write(ctx, 500, _loc["Error_Generic"], ex.Message);
            return;
        }
    }

    private static async Task Write(HttpContext ctx, int status, string title, string? detail)
    {
        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/problem+json";
        var payload = new
        {
            type = "about:blank",
            title,
            status,
            detail,
            traceId = ctx.TraceIdentifier
        };
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}