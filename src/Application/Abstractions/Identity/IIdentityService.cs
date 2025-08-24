using Application.Common;

namespace Application.Abstractions.Identity;

public interface IIdentityService
{
    Task<Result<Guid>> CreateUserAsync(string email, string userName, string password);
    Task AssignRoleAsync(Guid userId, string role);
    Task<Guid?> GetUserIdByEmailAsync(string email);
    Task<bool> CheckPasswordAsync(Guid userId, string password);
    Task<IReadOnlyList<string>> GetUserRolesAsync(Guid userId);
    Task<(Guid userId, string userName, string email)> GetUserProfileAsync(Guid userId);
}
