using System.Security.Claims;
using System.Text.Json;
using Application.Abstractions;
using Application.Features.Products.Commands.CreateProduct;
using Application.Features.Products.Commands.DeleteProduct;
using Application.Features.Products.Commands.UpdateProduct;
using Application.Features.Products.Queries.GetProductById;
using Application.Features.Products.Queries.GetProducts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICacheService _cache;

    public ProductsController(IMediator mediator, ICacheService cache)
    {
        _mediator = mediator;
        _cache = cache;
    }

    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> List([FromQuery] GetProductsQuery query, CancellationToken ct)
    {

        (object Result, string ETag) data = await _mediator.Send(query, ct);
        var result = data.Result;
        var etag = data.ETag;

        var ifNone = Request.Headers.IfNoneMatch.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(ifNone) && string.Equals(ifNone.Trim('"'), etag, StringComparison.Ordinal))
            return StatusCode(StatusCodes.Status304NotModified);

        Response.Headers.ETag = $"\"{etag}\"";

        var userKey = User.Identity?.IsAuthenticated == true
            ? User.IsInRole("Admin") ? "admin" : User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "user"
            : "anon";

        var cacheKey = $"products:list:{userKey}:{etag}";
        var cached = await _cache.GetStringAsync(cacheKey, ct);
        if (!string.IsNullOrEmpty(cached))
            return Content(cached, "application/json");

        var json = JsonSerializer.Serialize(result);
        await _cache.SetStringAsync(cacheKey, json, TimeSpan.FromSeconds(30), ct);

        return Content(json, "application/json");
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        => Ok(await _mediator.Send(new GetProductByIdQuery(id), ct));


    [HttpPost]
    [Authorize(Policy = "CanWriteProducts")]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand cmd, CancellationToken ct)
    {
        var id = await _mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(Get), new { id, version = "1.0" }, new { id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanWriteProducts")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand body, CancellationToken ct)
    {
        await _mediator.Send(new UpdateProductCommand(id, body.Name, body.Price), ct);
        return NoContent();
    }

    //dökümanda policy eklenmesi belirtilmemiş normalde eklenmeli bence.
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteProductCommand(id), ct);
        return NoContent();
    }
}
