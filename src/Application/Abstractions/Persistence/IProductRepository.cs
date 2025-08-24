using Application.Common.Specifications;
using Domain.Entities;
using System.Linq.Expressions;

namespace Application.Abstractions.Persistence;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(Product product, CancellationToken ct);
    Task UpdateAsync(Product product, CancellationToken ct);
    Task DeleteAsync(Product product, CancellationToken ct);

    Task<IReadOnlyList<Product>> ListAsync(ISpecification<Product> spec, CancellationToken ct);
    Task<int> CountAsync(ISpecification<Product> spec, CancellationToken ct);
    Task<TResult?> MaxAsync<TResult>(ISpecification<Product> spec, Expression<Func<Product, TResult>> selector, CancellationToken ct);
}
