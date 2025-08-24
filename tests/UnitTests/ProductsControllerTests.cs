using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Api.Controllers;
using Application.Abstractions;
using Application.Features.Products.Commands.CreateProduct;
using Application.Features.Products.Commands.DeleteProduct;
using Application.Features.Products.Commands.UpdateProduct;
using Application.Features.Products.Dtos;
using Application.Features.Products.Queries.GetProducts;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace UnitTests.Api;

public class ProductsControllerTests
{
    private static ProductsController CreateController(
        IMediator mediator,
        ICacheService cache,
        ClaimsPrincipal? user = null)
    {
        var c = new ProductsController(mediator, cache)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        if (user != null)
            c.ControllerContext.HttpContext.User = user;

        return c;
    }

    [Fact]
    public async Task Create_Should_Return_201_With_Location()
    {
        var mediator = new Mock<IMediator>();
        var cache = new Mock<ICacheService>();

        var newId = Guid.NewGuid();
        mediator.Setup(m => m.Send(It.IsAny<CreateProductCommand>(), default))
                .ReturnsAsync(newId);

        var controller = CreateController(mediator.Object, cache.Object);

        var result = await controller.Create(new CreateProductCommand("X", 10), default);
        var created = result as CreatedAtActionResult;

        created!.ActionName.Should().Be(nameof(ProductsController.Get));
        ((Guid)created.RouteValues!["id"]).Should().Be(newId);
        ((string)created.RouteValues!["version"]).Should().Be("1.0");
    }

    [Fact]
    public async Task Update_Should_Return_204()
    {
        var mediator = new Mock<IMediator>();
        var cache = new Mock<ICacheService>();

        mediator.Setup(m => m.Send(It.IsAny<UpdateProductCommand>(), default))
                .ReturnsAsync(Unit.Value);

        var controller = CreateController(mediator.Object, cache.Object);

        var result = await controller.Update(Guid.NewGuid(), new UpdateProductCommand(Guid.Empty, "Y", 2), default);
        (result as NoContentResult)!.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Fact]
    public async Task Delete_Should_Return_204()
    {
        var mediator = new Mock<IMediator>();
        var cache = new Mock<ICacheService>();

        mediator.Setup(m => m.Send(It.IsAny<DeleteProductCommand>(), default))
                .ReturnsAsync(Unit.Value);

        var controller = CreateController(mediator.Object, cache.Object);

        var result = await controller.Delete(Guid.NewGuid(), default);
        (result as NoContentResult)!.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }
}