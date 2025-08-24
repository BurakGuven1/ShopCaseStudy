using Application.Features.Orders;
using Application.Features.Orders.Commands.CreateOrder;
using Application.Features.Orders.Queries.GetOrders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Api.Controllers.OrdersController.CreateOrderRequest;

namespace Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/orders")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    public OrdersController(IMediator mediator) => _mediator = mediator;

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var uid = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        var result = await _mediator.Send(new GetOrdersQuery(uid, isAdmin), ct);
        return Ok(result);
    }

    [Authorize(Policy = "OrderOwnerOrAdmin")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var uid = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        var result = await _mediator.Send(new GetOrderByIdQuery(id, uid, isAdmin), ct);
        return Ok(result);
    }

    public record CreateOrderRequest(List<Item> Items)
    {
        public record Item(Guid ProductId, int Quantity);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest req, CancellationToken ct)
    {
        var uid = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var id = await _mediator.Send(new CreateOrderCommand(
            uid, req.Items.Select(i => (i.ProductId, i.Quantity)).ToList()), ct);

        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    [Authorize(Policy = "OrderOwnerOrAdmin")]
    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var uid = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        await _mediator.Send(new CancelOrderCommand(id, uid, isAdmin), ct);
        return NoContent();
    }
}
