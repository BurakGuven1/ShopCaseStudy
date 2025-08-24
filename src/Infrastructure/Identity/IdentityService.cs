using Application.Abstractions.Identity;
using Application.Common;
using Microsoft.AspNetCore.Identity;
using Infrastructure.Persistence;

namespace Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public IdentityService(UserManager<ApplicationUser> userManager,
                           RoleManager<IdentityRole<Guid>> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<Result<Guid>> CreateUserAsync(string email, string userName, string password)
    {
        var user = new ApplicationUser { Id = Guid.NewGuid(), Email = email, UserName = userName };
        var res = await _userManager.CreateAsync(user, password);

        return res.Succeeded
            ? Result<Guid>.Success(user.Id)
            : Result<Guid>.Failure(string.Join("; ", res.Errors.Select(e => e.Description)));
    }


    public async Task AssignRoleAsync(Guid userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("User not found");

        if (!await _roleManager.RoleExistsAsync(role))
            await _roleManager.CreateAsync(new IdentityRole<Guid>(role));

        await _userManager.AddToRoleAsync(user, role);
    }

    public async Task<Guid?> GetUserIdByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user?.Id;
    }

    public async Task<bool> CheckPasswordAsync(Guid userId, string password)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is not null && await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<IReadOnlyList<string>> GetUserRolesAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("User not found");
        var roles = await _userManager.GetRolesAsync(user);
        return roles.ToList();
    }

    public async Task<(Guid userId, string userName, string email)> GetUserProfileAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("User not found");
        return (user.Id, user.UserName ?? user.Email ?? "", user.Email ?? "");
    }
}
