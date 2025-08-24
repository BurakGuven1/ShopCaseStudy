using Application.Features.Dtos;
using MediatR;

namespace Application.Features.Orders;

public sealed record GetOrderByIdQuery(Guid Id, Guid RequestUserId, bool IsAdmin) : IRequest<OrderDto>;
