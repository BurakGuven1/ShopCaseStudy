using MediatR;

namespace Application.Features.Orders;

public sealed record CancelOrderCommand(Guid OrderId, Guid RequestUserId, bool IsAdmin) : IRequest<Unit>;
