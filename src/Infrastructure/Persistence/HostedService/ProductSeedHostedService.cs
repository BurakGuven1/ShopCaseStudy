using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence.HostedService;

public sealed class ProductSeedHostedService : IHostedService
{
    private readonly IServiceProvider _sp;
    private readonly IConfiguration _cfg;
    private readonly ILogger<ProductSeedHostedService> _logger;

    public ProductSeedHostedService(
        IServiceProvider sp,
        IConfiguration cfg,
        ILogger<ProductSeedHostedService> logger)
    {
        _sp = sp;
        _cfg = cfg;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        if (!string.Equals(_cfg["Seed:Products:Enabled"], "true", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Product seeding skipped (Seed:Products:Enabled != true).");
            return;
        }

        try
        {
            using var scope = _sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (!db.Products.Any())
            {
                _logger.LogInformation("Seeding default products...");
                var now = DateTimeOffset.UtcNow;
                var products = new[]
                {
                    new Product { Name = "Laptop A", Price = 1200m, UpdatedAt = now },
                    new Product { Name = "Tablet B", Price = 450m,  UpdatedAt = now },
                    new Product { Name = "Mouse C",  Price = 25m,   UpdatedAt = now }
                };

                await db.Products.AddRangeAsync(products, ct);
                await db.SaveChangesAsync(ct);

                _logger.LogInformation("Products seeded.");
            }
            else
            {
                _logger.LogInformation("Products already exist. Skipping seed.");
            }
        }
        catch (Exception)
        {
            _logger.LogWarning("Role seeding skipped (DB not available).");
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
