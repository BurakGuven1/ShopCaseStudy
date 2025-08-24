using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Identity;

public sealed class AdminUserSeedHostedService : IHostedService
{
    private readonly IServiceProvider _sp;
    private readonly IConfiguration _cfg;
    private readonly ILogger<AdminUserSeedHostedService> _logger;

    public AdminUserSeedHostedService(IServiceProvider sp, IConfiguration cfg, ILogger<AdminUserSeedHostedService> logger)
    {
        _sp = sp;
        _cfg = cfg;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        try
        {
            var email = _cfg["SeedAdmin:Email"];
            var pass = _cfg["SeedAdmin:Password"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pass))
            {
                _logger.LogInformation("Admin seeding skipped (SeedAdmin config missing).");
                return;
            }

            using var scope = _sp.CreateScope();
            var um = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var rm = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            var user = await um.FindByEmailAsync(email);
            if (user is null)
            {
                user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
                var create = await um.CreateAsync(user, pass);
                if (!create.Succeeded)
                {
                    _logger.LogWarning("Failed to create admin user: {Errors}", string.Join(",", create.Errors.Select(e => e.Description)));
                    return;
                }
                _logger.LogInformation("Admin user created: {Email}", email);
            }

            if (!await rm.RoleExistsAsync("Admin"))
                await rm.CreateAsync(new IdentityRole<Guid>("Admin"));

            if (!await um.IsInRoleAsync(user, "Admin"))
                await um.AddToRoleAsync(user, "Admin");
        }
        catch (Exception)
        {
            _logger.LogWarning("Role seeding skipped (DB not available).");
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
