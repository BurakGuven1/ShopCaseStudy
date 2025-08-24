namespace Application.Features.Dtos;

public sealed record OrderItemDto(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);
public sealed record OrderDto(Guid Id, Guid UserId, string Status, DateTimeOffset CreatedAt, IReadOnlyList<OrderItemDto> Items);
