using Application.Abstractions.Persistence;
using Application.Features.Orders;
using Application.Features.Orders.Commands.CreateOrder;
using Domain.Entities;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Application;

public class OrderHandlersTests
{
    [Fact]
    public async Task CreateOrder_Should_Validate_Products_And_Add()
    {
        var orders = new Mock<IOrderRepository>();
        var products = new Mock<IProductRepository>();

        var p1 = new Product { Id = Guid.NewGuid(), Name = "N", Price = 10 };
        products.Setup(r => r.GetByIdAsync(p1.Id, default)).ReturnsAsync(p1);

        orders.Setup(r => r.AddAsync(It.IsAny<Order>(), default))
              .Returns(Task.CompletedTask)
              .Callback<Order, CancellationToken>((o, _) =>
              {
                  o.Items.Should().HaveCount(1);
                  o.Items[0].ProductId.Should().Be(p1.Id);
              });

        var h = new CreateOrderHandler(orders.Object, products.Object);
        var id = await h.Handle(new CreateOrderCommand(Guid.NewGuid(), new() { (p1.Id, 2) }), default);

        id.Should().NotBeEmpty();
    }

    [Fact]
    public void Cancel_DomainRule_Should_Block_Shipped()
    {
        var o = new Order(Guid.NewGuid());
        o.AddItem(Guid.NewGuid(), "X", 1, 5);
        o.MarkShipped();

        FluentActions.Invoking(() => o.Cancel())
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*Shipped*");
    }
}
