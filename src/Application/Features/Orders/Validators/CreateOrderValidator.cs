using Application.Features.Orders.Commands.CreateOrder;
using FluentValidation;

namespace Application.Features.Orders;

public sealed class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.RequestUserId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).Must(i => i.Quantity > 0).WithMessage("Quantity must be greater than 0");
    }
}
