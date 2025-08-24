using FluentValidation;

namespace Application.Features.Auth.RegisterUser;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(100);
        RuleFor(x => x.UserName).NotEmpty().MinimumLength(3).MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
    }
}
