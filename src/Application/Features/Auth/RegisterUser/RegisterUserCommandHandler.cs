using Application.Abstractions.Identity;
using MediatR;

namespace Application.Features.Auth.RegisterUser;

public sealed class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IIdentityService _identity;
    public RegisterUserCommandHandler(IIdentityService identity) => _identity = identity;

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        var result = await _identity.CreateUserAsync(request.Email, request.UserName ?? request.Email, request.Password);
        if (!result.IsSuccess) throw new FluentValidation.ValidationException(result.Error ?? "Registration failed");

        var userId = result.Value;
        await _identity.AssignRoleAsync(userId, request.AsAdmin ? "Admin" : "User");
        return userId;
    }
}
