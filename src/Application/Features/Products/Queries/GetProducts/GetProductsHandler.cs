using Application.Abstractions;
using Application.Abstractions.Persistence;
using Application.Common;
using Application.Features.Products.Dtos;
using Application.Features.Products.Queries.GetProducts.Specs;
using MediatR;

namespace Application.Features.Products.Queries.GetProducts;

public sealed class GetProductsHandler : IRequestHandler<GetProductsQuery, (object Result, string ETag)>
{
    private readonly IProductRepository _repo;
    private readonly IEtagService _etag;

    public GetProductsHandler(IProductRepository repo, IEtagService etag)
    {
        _repo = repo; _etag = etag;
    }

    public async Task<(object Result, string ETag)> Handle(GetProductsQuery q, CancellationToken ct)
    {
        var listSpec = new ProductListSpec(q.Page, q.PageSize, q.NameContains, q.PriceMin, q.PriceMax, q.SortBy, q.SortDir, q.Cursor, q.UseCursor);
        var filterSpec = new ProductFilteredOnlySpec(q.NameContains, q.PriceMin, q.PriceMax);

        var items = await _repo.ListAsync(listSpec, ct);
        var total = await _repo.CountAsync(filterSpec, ct);

        var latestUpdated = await _repo.MaxAsync(filterSpec, p => (DateTimeOffset?)p.UpdatedAt, ct) ?? DateTimeOffset.MinValue;

        var etag = _etag.ComputeForProductsList(
            $"{q.Page}|{q.PageSize}|{q.NameContains}|{q.PriceMin}|{q.PriceMax}|{q.SortBy}|{q.SortDir}|{q.Cursor}|{q.UseCursor}",
            latestUpdated,
            total);

        var dtos = items.Select(p => new ProductDto(p.Id, p.Name, p.Price, p.UpdatedAt)).ToList();

        var size = q.PageSize <= 0 ? 20 : q.PageSize;

        if (q.UseCursor)
        {
            string? nextCursor = null;
            if (dtos.Count > 0 && dtos.Count == size)
            {
                var last = dtos[^1];
                var payload = $"{last.UpdatedAt.UtcTicks}|{last.Id}";
                nextCursor = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload));
            }

            var hasMore = dtos.Count == size;

            return (new CursorPage<ProductDto>(dtos, nextCursor, hasMore), etag);
        }

        var page = q.Page <= 0 ? 1 : q.Page;
        return (new PagedResult<ProductDto>(dtos, page, size, total), etag);
    }
}
