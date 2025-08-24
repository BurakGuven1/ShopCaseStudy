using MediatR;

namespace Application.Features.Auth.RegisterUser;

public sealed record RegisterUserCommand(string Email, string Password, string? UserName, bool AsAdmin) : IRequest<Guid>;
