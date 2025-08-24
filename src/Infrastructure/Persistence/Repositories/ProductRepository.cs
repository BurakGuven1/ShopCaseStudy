using Application.Abstractions.Persistence;
using Application.Common.Specifications;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Infrastructure.Persistence.Specifications;

namespace Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;
    public ProductRepository(AppDbContext db) => _db = db;

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Products.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(Product product, CancellationToken ct)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Product product, CancellationToken ct)
    {
        _db.Products.Update(product);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Product product, CancellationToken ct)
    {
        _db.Products.Remove(product);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> ListAsync(ISpecification<Product> spec, CancellationToken ct)
    {
        var query = SpecificationEvaluator.GetQuery(_db.Products, spec);
        return await query.ToListAsync(ct);
    }

    public async Task<int> CountAsync(ISpecification<Product> spec, CancellationToken ct)
    {
        var query = SpecificationEvaluator.GetQuery(_db.Products, new CountSpecWrapper<Product>(spec));
        return await query.CountAsync(ct);
    }

    public async Task<TResult?> MaxAsync<TResult>(ISpecification<Product> spec, Expression<Func<Product, TResult>> selector, CancellationToken ct)
    {
        var query = SpecificationEvaluator.GetQuery(_db.Products, new CountSpecWrapper<Product>(spec));
        var any = await query.AnyAsync(ct);
        if (!any) return default;
        return await query.MaxAsync(selector, ct);
    }

    private sealed class CountSpecWrapper<T> : ISpecification<T>
    {
        private readonly ISpecification<T> _inner;
        public CountSpecWrapper(ISpecification<T> inner) => _inner = inner;

        public System.Linq.Expressions.Expression<Func<T, bool>>? Criteria => _inner.Criteria;
        public List<System.Linq.Expressions.Expression<Func<T, object>>> Includes => new();
        public System.Linq.Expressions.Expression<Func<T, object>>? OrderBy => null;
        public System.Linq.Expressions.Expression<Func<T, object>>? OrderByDescending => null;
        public List<(System.Linq.Expressions.Expression<Func<T, object>> KeySelector, bool Desc)> ThenBys => new();
        public int? Skip => null;
        public int? Take => null;
        public bool AsNoTracking => true;
    }
}