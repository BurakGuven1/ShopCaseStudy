using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromServices] HealthCheckService hc, CancellationToken ct)
    {
        var report = await hc.CheckHealthAsync(ct);
        var payload = new
        {
            status = report.Status.ToString(),
            entries = report.Entries.Select(e => new {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                data = e.Value.Data
            })
        };

        return Ok(payload);
    }
}
