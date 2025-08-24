using Domain.Entities;

namespace Application.Abstractions.Persistence;

public interface IOrderRepository
{
    Task<(IReadOnlyList<Order> Items, int Total)> ListAsync(
        Guid? userIdFilter,
        int page,
        int pageSize,
        CancellationToken ct);

    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(Order order, CancellationToken ct);
    Task UpdateAsync(Order order, CancellationToken ct);
    Task<bool> IsOwnerAsync(Guid orderId, Guid userId, CancellationToken ct);
}
