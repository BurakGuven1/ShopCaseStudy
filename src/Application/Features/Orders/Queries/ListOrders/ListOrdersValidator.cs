using FluentValidation;

namespace Application.Features.Orders.Queries.ListOrders;

public sealed class ListOrdersValidator : AbstractValidator<ListOrdersQuery>
{
    public ListOrdersValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.RequestUserId).NotEmpty();
    }
}
