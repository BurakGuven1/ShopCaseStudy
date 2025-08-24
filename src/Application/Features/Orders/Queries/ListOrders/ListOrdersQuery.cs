using MediatR;

namespace Application.Features.Orders.Queries.ListOrders;

public sealed record ListOrdersQuery(
    Guid RequestUserId,
    bool IsAdmin,
    int Page = 1,
    int PageSize = 20
) : IRequest<(IReadOnlyList<ListOrdersDto> Items, int Total)>;

public sealed record ListOrdersDto(
    Guid Id,
    string Status,
    DateTimeOffset CreatedAt,
    decimal Total
);
