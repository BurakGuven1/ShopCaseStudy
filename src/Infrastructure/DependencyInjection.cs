using Application.Abstractions;
using Application.Abstractions.Identity;
using Application.Abstractions.Persistence;
using Application.Abstractions.Security;
using Infrastructure.Caching;
using Infrastructure.Etags;
using Infrastructure.Health;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Infrastructure.Persistence.HostedService;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Veritabanı bağlantı dizesi 'DefaultConnection' appsettings.json dosyasında bulunamadı veya boş.");
        }
        services.AddDbContext<AppDbContext>(opt =>
        opt.UseNpgsql(connectionString));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddScoped<IEtagService, SimpleEtagService>();

        var redisConn = configuration.GetConnectionString("Redis")
                      ?? configuration["ConnectionStrings:Redis"];

        if (!string.IsNullOrWhiteSpace(redisConn))
        {
            services.AddStackExchangeRedisCache(o => o.Configuration = redisConn);

            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConn));
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddHealthChecks()
        .AddCheck<DbAndRedisHealthCheck>("deps", tags: new[] { "ready" });


        services.AddScoped<ICacheService, DistributedCacheService>();
        services.AddHostedService<RoleSeedHostedService>();
        services.AddHostedService<AdminUserSeedHostedService>();
        services.AddHostedService<ProductSeedHostedService>();


        services
            .AddIdentityCore<ApplicationUser>(opt =>
            {
                opt.User.RequireUniqueEmail = true;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireUppercase = false;
                opt.Password.RequiredLength = 6;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        var jwt = configuration.GetSection("Jwt");
        var issuer = jwt["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer missing");
        var audience = jwt["Audience"] ?? throw new InvalidOperationException("Jwt:Audience missing");
        var key = jwt["Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
        if (key.Length < 32) throw new InvalidOperationException("Jwt:Key too short (>=32)");

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IJwtTokenGenerator>(_ => new JwtTokenGenerator(issuer, audience, signingKey));

        services.AddHostedService<RoleSeedHostedService>();

        return services;
    }
}