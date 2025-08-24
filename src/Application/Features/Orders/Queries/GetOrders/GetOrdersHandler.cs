// src/Application/Features/Orders/GetOrdersHandler.cs
using System.Linq; // OrderByDescending, Select
using Application.Abstractions.Persistence;
using Application.Features.Dtos;
using Application.Features.Orders.Queries.GetOrders;
using MediatR;

namespace Application.Features.Orders;

public sealed class GetOrdersHandler : IRequestHandler<GetOrdersQuery, IReadOnlyList<OrderDto>>
{
    private readonly IOrderRepository _repo;

    public GetOrdersHandler(IOrderRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<OrderDto>> Handle(GetOrdersQuery request, CancellationToken ct)
    {
        // Admin ise tümü, değilse sadece kendi siparişleri
        Guid? userFilter = request.IsAdmin ? (Guid?)null : request.RequestUserId;


        var (items, _) = await _repo.ListAsync(userFilter, page: 1, pageSize: int.MaxValue, ct);

        var list = items
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderDto(
                o.Id,
                o.UserId,
                o.Status.ToString(),
                o.CreatedAt,
                o.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)).ToList()
            ))
            .ToList();

        return list;
    }
}
