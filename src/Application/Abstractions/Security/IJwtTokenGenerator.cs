namespace Application.Abstractions.Security;

public interface IJwtTokenGenerator
{
    string CreateToken(Guid userId, string userName, string email, IEnumerable<string> roles);
}
