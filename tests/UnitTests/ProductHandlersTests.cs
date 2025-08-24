using Application.Abstractions;
using Application.Abstractions.Persistence;
using Application.Common.Specifications;
using Application.Features.Products.Commands.CreateProduct;
using Application.Features.Products.Commands.DeleteProduct;
using Application.Features.Products.Commands.UpdateProduct;
using Application.Features.Products.Dtos;
using Application.Features.Products.Queries.GetProducts;
using Domain.Entities;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Application;

public class ProductHandlersTests
{
    [Fact]
    public async Task Create_Update_Delete_Should_Increment_Version()
    {
        var repo = new Mock<IProductRepository>();
        var cache = new Mock<ICacheService>();

        cache.Setup(c => c.IncrementAsync("products:version", default))
             .ReturnsAsync(1L)
             .Verifiable();

        var create = new CreateProductHandler(repo.Object, cache.Object);
        var id = await create.Handle(new CreateProductCommand("P", 5), default);
        id.Should().NotBeEmpty();

        var p = new Product { Id = id, Name = "P", Price = 5 };
        repo.Setup(r => r.GetByIdAsync(id, default)).ReturnsAsync(p);
        var update = new UpdateProductHandler(repo.Object, cache.Object);
        await update.Handle(new UpdateProductCommand(id, "P2", 6), default);

        var del = new DeleteProductHandler(repo.Object, cache.Object);
        await del.Handle(new DeleteProductCommand(id), default);

        cache.Verify(c => c.IncrementAsync("products:version", default), Times.Exactly(3));
    }
}