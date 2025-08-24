using System.Security.Claims;
using Application.Abstractions.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Api.Auth;

public sealed class OrderOwnerOrAdminRequirement : IAuthorizationRequirement { }

public sealed class OrderOwnerOrAdminHandler : AuthorizationHandler<OrderOwnerOrAdminRequirement>
{
    private readonly IOrderRepository _orders;

    public OrderOwnerOrAdminHandler(IOrderRepository orders)
    {
        _orders = orders;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrderOwnerOrAdminRequirement requirement)
    {
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        if (context.Resource is not HttpContext httpContext)
            return;

        var routeData = httpContext.GetRouteData();
        if (!routeData.Values.TryGetValue("id", out var idObj) ||
            idObj is null ||
            !Guid.TryParse(idObj.ToString(), out var orderId))
            return;

        var sub = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (sub is null || !Guid.TryParse(sub, out var userId))
            return;

        var ct = httpContext.RequestAborted;

        if (await _orders.IsOwnerAsync(orderId, userId, ct))
            context.Succeed(requirement);
    }
}
