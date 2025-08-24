using Application.Abstractions.Persistence;
using Domain.Entities;
using MediatR;

namespace Application.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orders;
    private readonly IProductRepository _products;

    public CreateOrderHandler(IOrderRepository orders, IProductRepository products)
    {
        _orders = orders;
        _products = products;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        if (request.Items.Count == 0)
            throw new FluentValidation.ValidationException("Items is required");

        var order = new Order(request.RequestUserId);

        foreach (var (productId, qty) in request.Items)
        {
            var p = await _products.GetByIdAsync(productId, ct) ?? throw new KeyNotFoundException("Product not found");
            order.AddItem(p.Id, p.Name, qty, p.Price);
        }

        await _orders.AddAsync(order, ct);
        return order.Id;
    }
}
