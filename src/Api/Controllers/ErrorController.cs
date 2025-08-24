using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Route("error")]
public class ErrorController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public ErrorController(IWebHostEnvironment env) => _env = env;

    [HttpGet, HttpPost, HttpPut, HttpDelete]
    public IActionResult Handle()
    {
        var feature = HttpContext.Features.Get<IExceptionHandlerFeature>();
        var ex = feature?.Error;

        return Problem(
            statusCode: 500,
            title: ex?.Message ?? "Unexpected error",
            detail: _env.IsDevelopment() ? ex?.StackTrace : null
        );
    }
}
