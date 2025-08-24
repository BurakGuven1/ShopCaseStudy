using System.Linq.Expressions;

namespace Application.Common.Specifications;

public abstract class BaseSpecification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>>? Criteria { get; protected init; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; protected set; }
    public Expression<Func<T, object>>? OrderByDescending { get; protected set; }
    public List<(Expression<Func<T, object>> KeySelector, bool Desc)> ThenBys { get; } = new();
    public int? Skip { get; protected set; }
    public int? Take { get; protected set; }
    public bool AsNoTracking { get; protected set; } = true;

    protected void AddInclude(Expression<Func<T, object>> includeExpression) => Includes.Add(includeExpression);
    protected void ApplyPaging(int skip, int take) { Skip = skip; Take = take; }
    protected void ApplyOrderBy(Expression<Func<T, object>> keySelector) => OrderBy = keySelector;
    protected void ApplyOrderByDescending(Expression<Func<T, object>> keySelector) => OrderByDescending = keySelector;
    protected void ThenBy(Expression<Func<T, object>> keySelector, bool desc = false) => ThenBys.Add((keySelector, desc));
}
