using Application.Abstractions.Persistence;
using MediatR;

namespace Application.Features.Orders;

public sealed class CancelOrderHandler : IRequestHandler<CancelOrderCommand, Unit>
{
    private readonly IOrderRepository _orders;

    public CancelOrderHandler(IOrderRepository orders) => _orders = orders;

    public async Task<Unit> Handle(CancelOrderCommand request, CancellationToken ct)
    {
        var o = await _orders.GetByIdAsync(request.OrderId, ct) ?? throw new KeyNotFoundException("Order not found");
        if (!request.IsAdmin && o.UserId != request.RequestUserId)
            throw new UnauthorizedAccessException("Not owner");

        o.Cancel();
        await _orders.UpdateAsync(o, ct);
        return Unit.Value;
    }
}
