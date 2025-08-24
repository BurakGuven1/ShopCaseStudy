using Application.Common.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Specifications;

internal static class SpecificationEvaluator
{
    public static IQueryable<T> GetQuery<T>(IQueryable<T> inputQuery, ISpecification<T> spec)
        where T : class
    {
        var query = inputQuery;

        if (spec.AsNoTracking)
            query = query.AsNoTracking();

        if (spec.Criteria is not null)
            query = query.Where(spec.Criteria);

        query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));

        IOrderedQueryable<T>? orderedQuery = null;
        if (spec.OrderBy is not null)
            orderedQuery = query.OrderBy(spec.OrderBy);
        else if (spec.OrderByDescending is not null)
            orderedQuery = query.OrderByDescending(spec.OrderByDescending);

        if (orderedQuery is not null)
        {
            foreach (var (keySelector, isDescending) in spec.ThenBys)
            {
                orderedQuery = isDescending
                    ? orderedQuery.ThenByDescending(keySelector)
                    : orderedQuery.ThenBy(keySelector);
            }
            query = orderedQuery;
        }

        if (spec.Skip.HasValue) query = query.Skip(spec.Skip.Value);
        if (spec.Take.HasValue) query = query.Take(spec.Take.Value);

        return query;
    }
}