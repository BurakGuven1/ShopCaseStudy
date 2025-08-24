using Application.Abstractions.Persistence;
using MediatR;
using Application.Features.Dtos;

namespace Application.Features.Orders;

public sealed class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IOrderRepository _repo;
    public GetOrderByIdHandler(IOrderRepository repo) => _repo = repo;

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken ct)
    {
        var o = await _repo.GetByIdAsync(request.Id, ct) ?? throw new KeyNotFoundException("Order not found");
        if (!request.IsAdmin && o.UserId != request.RequestUserId)
            throw new UnauthorizedAccessException("Not owner");

        var dto = new OrderDto(
            o.Id,
            o.UserId,
            o.Status.ToString(),
            o.CreatedAt,
            o.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)).ToList()
        );

        return dto;
    }
}
