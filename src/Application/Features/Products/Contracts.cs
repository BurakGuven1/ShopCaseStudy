using MediatR;
using Application.Common;

namespace Application.Features.Products;

public record ProductDto(Guid Id, string Name, decimal Price, DateTimeOffset UpdatedAt);

public record GetProductsQuery(
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

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;

public record CreateProductCommand(string Name, decimal Price) : IRequest<Guid>;
public record UpdateProductCommand(Guid Id, string Name, decimal Price) : IRequest<Unit>;
public record DeleteProductCommand(Guid Id) : IRequest<Unit>;
