using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Identity;

public sealed class RoleSeedHostedService : IHostedService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<RoleSeedHostedService> _logger;

    public RoleSeedHostedService(IServiceProvider sp, ILogger<RoleSeedHostedService> logger)
    {
        _sp = sp;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _sp.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            foreach (var role in new[] { "Admin", "User" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var res = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                    if (res.Succeeded)
                        _logger.LogInformation("Role {Role} created.", role);
                    else
                        _logger.LogWarning("Failed to create role {Role}: {Errors}",
                            role, string.Join(",", res.Errors.Select(e => e.Description)));
                }
            }
        }
        catch (Exception)
        {
            _logger.LogWarning("Role seeding skipped (DB not available).");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
