using Application.Abstractions.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;
    public OrderRepository(AppDbContext db) => _db = db;

    public async Task<(IReadOnlyList<Order> Items, int Total)> ListAsync(
        Guid? userIdFilter,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        var baseQuery = _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .AsQueryable();

        if (userIdFilter.HasValue)
            baseQuery = baseQuery.Where(o => o.UserId == userIdFilter.Value);

        var total = await baseQuery.CountAsync(ct);

        var p = page <= 0 ? 1 : page;
        var s = pageSize <= 0 ? 20 : pageSize;

        var items = await baseQuery
            .OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id)
            .Skip((p - 1) * s)
            .Take(s)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task AddAsync(Order order, CancellationToken ct)
    {
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);
        _db.Entry(order).State = EntityState.Detached;
    }

    public async Task UpdateAsync(Order order, CancellationToken ct)
    {
        _db.Orders.Update(order);
        await _db.SaveChangesAsync(ct);
        _db.Entry(order).State = EntityState.Detached;
    }

    public Task<bool> IsOwnerAsync(Guid orderId, Guid userId, CancellationToken ct)
        => _db.Orders.AsNoTracking().AnyAsync(o => o.Id == orderId && o.UserId == userId, ct);
}
