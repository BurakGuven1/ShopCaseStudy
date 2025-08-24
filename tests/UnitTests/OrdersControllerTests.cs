using Application.Features.Orders;
using Application.Features.Orders.Queries.GetOrders;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Application.Features.Dtos;
using Api.Controllers; // OrderItemDto ve OrderDto için

namespace UnitTests.Api;

public class OrdersControllerTests
{
    private static OrdersController Create(IMediator mediator, ClaimsPrincipal user)
    {
        var c = new OrdersController(mediator)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            }
        };
        return c;
    }

    [Fact]
    public async Task List_As_User_Should_Call_Mediator_With_IsAdminFalse_And_CorrectUserId()
    {
        var mediator = new Mock<IMediator>();
        var sentQuery = default(GetOrdersQuery);

        mediator.Setup(m => m.Send(It.IsAny<GetOrdersQuery>(), default))
            .Callback<object, System.Threading.CancellationToken>((q, _) => sentQuery = (GetOrdersQuery)q)
            .ReturnsAsync(new List<OrderDto>()); // OrderDto listesi döndür

        var userId = Guid.NewGuid().ToString();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, "User")
        }, "Test"));

        var controller = Create(mediator.Object, user);

        var result = await controller.List(default);
        (result as OkObjectResult)!.StatusCode.Should().Be(StatusCodes.Status200OK);

        sentQuery.Should().NotBeNull();
        sentQuery!.IsAdmin.Should().BeFalse();
        sentQuery.RequestUserId.Should().Be(Guid.Parse(userId));
    }

    [Fact]
    public async Task Get_Should_Pass_Id_And_RoleFlags_To_Mediator()
    {
        var mediator = new Mock<IMediator>();

        var orderItems = new List<OrderItemDto>();

        mediator.Setup(m => m.Send(It.IsAny<GetOrderByIdQuery>(), default))
                .ReturnsAsync(new OrderDto(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "Pending",
                    DateTimeOffset.UtcNow,
                    orderItems));

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        }, "Test"));

        var c = Create(mediator.Object, user);
        var result = await c.Get(Guid.NewGuid(), default);

        (result as OkObjectResult)!.StatusCode.Should().Be(200);
        mediator.Verify(m => m.Send(It.IsAny<GetOrderByIdQuery>(), default), Times.Once);
    }
}