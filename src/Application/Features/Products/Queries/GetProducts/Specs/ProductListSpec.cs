using Application.Common;
using Application.Common.Specifications;
using Domain.Entities;
using System.Linq.Expressions;

namespace Application.Features.Products.Queries.GetProducts.Specs;

public sealed class ProductListSpec : BaseSpecification<Product>
{
    public ProductListSpec(
        int page,
        int pageSize,
        string? nameContains,
        decimal? priceMin,
        decimal? priceMax,
        string? sortBy,
        string? sortDir,
        string? cursor,
        bool useCursor)
    {
        Expression<Func<Product, bool>> filter = p =>
            (string.IsNullOrEmpty(nameContains) || p.Name.Contains(nameContains)) &&
            (!priceMin.HasValue || p.Price >= priceMin.Value) &&
            (!priceMax.HasValue || p.Price <= priceMax.Value);

        if (useCursor && !string.IsNullOrWhiteSpace(cursor))
        {
            try
            {
                var parts = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(cursor)).Split('|');
                if (parts.Length == 2 && long.TryParse(parts[0], out var ticks) && Guid.TryParse(parts[1], out var lastId))
                {
                    var lastUpdatedAt = new DateTimeOffset(ticks, TimeSpan.Zero);
                    Expression<Func<Product, bool>> cursorFilter = p =>
                        p.UpdatedAt < lastUpdatedAt ||
                        (p.UpdatedAt == lastUpdatedAt && p.Id.CompareTo(lastId) < 0);

                    filter = filter.And(cursorFilter);
                }
            }
            catch (FormatException) { }
        }

        Criteria = filter;

        var by = (sortBy ?? "updatedAt").ToLowerInvariant();
        var dir = (sortDir ?? "desc").ToLowerInvariant();
        bool isDescending = dir == "desc";

        switch (by)
        {
            case "price":
                if (isDescending) ApplyOrderByDescending(p => p.Price); else ApplyOrderBy(p => p.Price);
                break;
            case "name":
                if (isDescending) ApplyOrderByDescending(p => p.Name); else ApplyOrderBy(p => p.Name);
                break;
            default: // updatedAt
                if (isDescending) ApplyOrderByDescending(p => p.UpdatedAt); else ApplyOrderBy(p => p.UpdatedAt);
                break;
        }
        ThenBy(p => p.Id, isDescending);

        var size = pageSize <= 0 ? 20 : pageSize;
        if (useCursor)
        {
            Take = size;
        }
        else
        {
            var pageIndex = page <= 0 ? 1 : page;
            ApplyPaging((pageIndex - 1) * size, size);
        }
    }
}