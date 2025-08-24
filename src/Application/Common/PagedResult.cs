namespace Application.Common;

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);

public record CursorPage<T>(IReadOnlyList<T> Items, string? NextCursor);
