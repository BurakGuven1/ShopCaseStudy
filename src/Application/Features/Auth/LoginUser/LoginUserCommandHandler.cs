using Application.Abstractions.Identity;
using Application.Abstractions.Security;
using MediatR;

namespace Application.Features.Auth.LoginUser;

public sealed class LoginUserCommandHandler
    : IRequestHandler<LoginUserCommand, string>
{
    private readonly IIdentityService _identity;
    private readonly IJwtTokenGenerator _jwt;
    public LoginUserCommandHandler(IIdentityService identity, IJwtTokenGenerator jwt)
    { _identity = identity; _jwt = jwt; }

    public async Task<string> Handle(LoginUserCommand request, CancellationToken ct)
    {
        var userId = await _identity.GetUserIdByEmailAsync(request.Email)
                     ?? throw new UnauthorizedAccessException("Invalid credentials");

        var ok = await _identity.CheckPasswordAsync(userId, request.Password);
        if (!ok) throw new UnauthorizedAccessException("Invalid credentials");

        var roles = await _identity.GetUserRolesAsync(userId);
        var profile = await _identity.GetUserProfileAsync(userId);
        return _jwt.CreateToken(userId, profile.userName, profile.email, roles);
    }
}
