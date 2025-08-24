using FluentValidation;

namespace Application.Features.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdQueryValidator : AbstractValidator<GetOrderByIdQuery>
{
    public GetOrderByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.RequestUserId).NotEmpty();
    }
}
