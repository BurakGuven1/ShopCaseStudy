using MediatR;

namespace Application.Features.Auth.LoginUser;

public sealed record LoginUserCommand(string Email, string Password) : IRequest<string>;
