using MediatR;

namespace Application.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    Guid RequestUserId,
    List<(Guid ProductId, int Quantity)> Items
) : IRequest<Guid>;
