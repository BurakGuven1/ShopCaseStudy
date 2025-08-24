using System.Linq;

namespace Application.Common.Specifications;

public interface ICursorSpecification<T>
{
    /// <summary>Null değilse cursor uygulanmalıdır.</summary>
    object? CursorValue { get; }

    /// <summary>Cursor mantığını sorguya uygular (ör. strict-seek).</summary>
    IQueryable<T> ApplyCursor(IQueryable<T> query);
}
