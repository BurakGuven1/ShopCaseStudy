using MediatR;

namespace Application.Features.Products.Queries.GetProducts;

public sealed record GetProductsQuery(
    int Page = 1,
    int PageSize = 20,
    string? NameContains = null,
    decimal? PriceMin = null,
    decimal? PriceMax = null,
    string? SortBy = "updatedAt",
    string? SortDir = "desc",
    string? Cursor = null,
    bool UseCursor = false
) : IRequest<(object Result, string ETag)>;
