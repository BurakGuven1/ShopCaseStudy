using Application.Features.Dtos;
using MediatR;

namespace Application.Features.Orders.Queries.GetOrders;

public sealed record GetOrdersQuery(Guid RequestUserId, bool IsAdmin) : IRequest<IReadOnlyList<OrderDto>>;
