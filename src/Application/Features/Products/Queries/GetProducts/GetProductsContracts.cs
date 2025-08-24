namespace Application.Features.Products.Queries.GetProducts;

public sealed record GetProductsRequest(
    int? Page,
    int? PageSize,
    string? Name,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? SortBy,
    string? SortDir,
    string? Cursor,
    bool UseCursor = false
);

public sealed record CursorPage<T>(
    IReadOnlyList<T> Items,
    string? NextCursor,
    bool HasMore
);
