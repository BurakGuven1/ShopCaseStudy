using Application.Abstractions.Persistence;
using MediatR;

namespace Application.Features.Orders.Queries.ListOrders;

public sealed class ListOrdersHandler
    : IRequestHandler<ListOrdersQuery, (IReadOnlyList<ListOrdersDto> Items, int Total)>
{
    private readonly IOrderRepository _orders;
    public ListOrdersHandler(IOrderRepository orders) => _orders = orders;

    public async Task<(IReadOnlyList<ListOrdersDto>, int)> Handle(ListOrdersQuery q, CancellationToken ct)
    {
        var filterUserId = q.IsAdmin ? (Guid?)null : q.RequestUserId;

        var (entities, total) = await _orders.ListAsync(filterUserId, q.Page, q.PageSize, ct);

        var items = entities.Select(o => new ListOrdersDto(
            o.Id,
            o.Status.ToString(),
            o.CreatedAt,
            o.Items.Sum(i => i.UnitPrice * i.Quantity)
        )).ToList();

        return (items, total);
    }
}
